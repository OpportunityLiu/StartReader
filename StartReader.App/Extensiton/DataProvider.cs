using Newtonsoft.Json;
using Opportunity.MvvmUniverse;
using StartReader.DataExchange.Request;
using StartReader.DataExchange.Response;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace StartReader.App.Extensiton
{
    [DebuggerDisplay(@"Id = {Id} DispName = {DisplayName} Service = {AppServiceName}")]
    class DataProvider : ObservableObject
    {
        public string Id { get; }
        public DataProviderSource Source { get; }
        public IPropertySet Properties { get; }

        private bool isAvailable;
        public bool IsAvailable { get => this.isAvailable; private set => Set(ref this.isAvailable, value); }

        private string displayName;
        public string DisplayName { get => this.displayName; private set => Set(ref this.displayName, value); }

        private Uri url;
        public Uri Url { get => this.url; private set => Set(ref this.url, value); }

        private string appServiceName;
        public string AppServiceName { get => this.appServiceName; private set => Set(ref this.appServiceName, value); }

        private string description;
        public string Description { get => this.description; private set => Set(ref this.description, value); }

        public DataProvider(DataProviderSource source, IPropertySet providerData)
        {
            Source = source;
            Id = providerData.GetProperty(nameof(Id));
            if (Id.IsNullOrWhiteSpace())
                throw new ArgumentNullException("Id of Provider is not defined or empty");
            this.Properties = providerData;
            LoadData(providerData);
        }

        public void LoadData(IPropertySet providerData)
        {
            Debug.Assert(providerData.GetProperty(nameof(Id)) == Id);
            try
            {
                DisplayName = providerData.GetProperty(nameof(DisplayName)).CoalesceNullOrWhiteSpace(Id);
                AppServiceName = providerData.GetProperty(nameof(AppServiceName));
                if (AppServiceName.IsNullOrWhiteSpace())
                    throw new ArgumentNullException("AppServiceName of Provider is not defined or empty");
                Url = new Uri(providerData.GetProperty(nameof(Url)));
                Description = string.Join(Environment.NewLine, providerData.GetProperties(nameof(Description))).CoalesceNullOrWhiteSpace("");
            }
            catch
            {
                IsAvailable = false;
                return;
            }
            IsAvailable = true;
        }

        private AppServiceConnection connection;
        private bool IsOpened => this.connection != null;

        public async Task OpenAsync()
        {
            if (!IsAvailable)
                throw new InvalidOperationException("该扩展无法使用。");
            if (IsOpened)
                return;

            var connection = new AppServiceConnection
            {
                AppServiceName = AppServiceName,
                PackageFamilyName = Source.PackageFamilyName,
            };
            var status = await connection.OpenAsync();
            var info = default(string);
            switch (status)
            {
            case AppServiceConnectionStatus.AppNotInstalled:
                info = "尝试连接的应用服务的包未安装在设备上";
                break;
            case AppServiceConnectionStatus.AppUnavailable:
                info = "尝试连接的应用服务的包暂时不可用";
                break;
            case AppServiceConnectionStatus.AppServiceUnavailable:
                info = "具有指定包系列名称的应用已安装并且可用，但该应用未声明对指定应用服务的支持";
                break;
            }
            if (status != AppServiceConnectionStatus.Success)
            {
                throw new InvalidOperationException($"App Service 连接失败，{info}({status})。");
            }
            if (Interlocked.CompareExchange(ref this.connection, connection, null) is null)
            {
                connection.ServiceClosed += this.Connection_ServiceClosed;
            }
            OnPropertyChanged(nameof(IsOpened));
        }

        private void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            this.Close();
        }

        public void Close()
        {
            var connection = Interlocked.Exchange(ref this.connection, null);
            if (connection is null)
                return;
            OnPropertyChanged(nameof(IsOpened));
            connection.Dispose();
        }

        public async Task<TResponse> ExecuteAsync<TRequest, TResponse>(IRequestMessage<TRequest, TResponse> message)
            where TRequest : IRequestMessage<TRequest, TResponse>
            where TResponse : IResponseMessage<TRequest, TResponse>
        {
            if (!IsOpened)
                await OpenAsync();
            if (message is RequestMessageBase mb)
                mb.ProviderId = this.Id;
            var response = await this.connection.SendMessageAsync(new ValueSet
            {
                ["Method"] = message.Method,
                ["Data"] = JsonConvert.SerializeObject(message),
            });
            var info = default(string);
            switch (response.Status)
            {
            case AppServiceResponseStatus.Failure:
                info = "应用服务未能接收和处理消息";
                break;
            case AppServiceResponseStatus.ResourceLimitsExceeded:
                info = "应用服务已退出，因为可用的资源不够";
                break;
            case AppServiceResponseStatus.MessageSizeTooLarge:
                info = "应用无法处理消息，因为它太大";
                break;
            }
            if (response.Status != AppServiceResponseStatus.Success)
                throw new InvalidOperationException($"App Service 调用失败，{info}({response.Status})。");
            try
            {
                var msg = response.Message;
                var code = Convert.ToInt32(msg["Code"]);
                msg.TryGetValue("Message", out var error);
                if (code != 0)
                    throw new InvalidOperationException($"{error}（错误代码：{code}）");
                var data = Convert.ToString(msg["Data"]);
                return JsonConvert.DeserializeObject<TResponse>(data);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"App Service 返回数据有误。", ex);
            }
        }
    }
}
