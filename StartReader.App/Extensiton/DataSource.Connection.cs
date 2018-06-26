using Newtonsoft.Json;
using StartReader.DataExchange.Request;
using StartReader.DataExchange.Response;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace StartReader.App.Extensiton
{
    partial class DataSource
    {
        private AppServiceConnection connection;
        public bool IsOpened => this.connection != null;

        public async Task OpenAsync()
        {
            if (!IsAvailable)
                throw new InvalidOperationException("该扩展无法使用。");
            if (IsOpened)
                return;

            var connection = new AppServiceConnection
            {
                AppServiceName = AppServiceName,
                PackageFamilyName = PackageFamilyName,
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
                LastUse = DateTime.UtcNow;
            }
            OnPropertyChanged(nameof(IsOpened));
        }

        private void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            this.Close();
        }

        internal DateTime LastUse { get; private set; }

        public void Close()
        {
            var connection = Interlocked.Exchange(ref this.connection, null);
            if (connection is null)
                return;
            OnPropertyChanged(nameof(IsOpened));
            connection.Dispose();
        }

        public async Task<TResponse> ExecuteAsync<TRequest, TResponse>(IRequestMessage<TRequest, TResponse> message)
            where TRequest : RequestMessageBase, IRequestMessage<TRequest, TResponse>
            where TResponse : ResponseMessageBase, IResponseMessage<TRequest, TResponse>
        {
            if (!IsOpened)
                await OpenAsync();
            else
                LastUse = DateTime.UtcNow;

            if (message is RequestMessageBase mb)
                mb.ProviderId = this.ExtensionId;
            Debug.Assert(message.ProviderId == this.ExtensionId);
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
                if (code != 0)
                {
                    msg.TryGetValue("Message", out var error);
                    throw new DataSourceException($"{error}（错误代码：{code}）");
                }
                var data = Convert.ToString(msg["Data"]);
                var r = JsonConvert.DeserializeObject<TResponse>(data);
                r.Source = this;
                return r;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (DataSourceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"App Service 返回数据有误。", ex);
            }
        }
    }

    public class DataSourceException : Exception
    {
        public DataSourceException() { }
        public DataSourceException(string message) : base(message) { }
        public DataSourceException(string message, Exception inner) : base(message, inner) { }
    }
}
