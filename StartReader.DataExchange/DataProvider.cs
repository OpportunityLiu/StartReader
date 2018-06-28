using StartReader.DataExchange.Request;
using StartReader.DataExchange.Response;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;

namespace StartReader.DataExchange
{
    public static class DataProviderExtension
    {
        public static DataProviderFactory AttachProvider(this AppServiceTriggerDetails triggerDetails)
        {
            return new DataProviderFactory(triggerDetails);
        }

        public sealed class DataProviderFactory
        {
            private readonly AppServiceTriggerDetails triggerDetails;

            public DataProvider Provider { get; private set; }

            public DataProviderFactory(AppServiceTriggerDetails triggerDetails) => this.triggerDetails = triggerDetails;

            public DataProviderFactory Add<T>(string serviceName)
                where T : DataProvider, new()
            {
                if (serviceName == this.triggerDetails.Name)
                    Provider = DataProvider.Create<T>(this.triggerDetails.AppServiceConnection);
                return this;
            }

            public DataProviderFactory Add<T>(string serviceName, Func<T> factory)
                where T : DataProvider
            {
                if (factory is null)
                    throw new ArgumentNullException(nameof(factory));
                if (serviceName == this.triggerDetails.Name)
                    Provider = DataProvider.Create(this.triggerDetails.AppServiceConnection, factory);
                return this;
            }
        }
    }

    public abstract class DataProvider
    {
        public static T Create<T>(AppServiceConnection connection)
            where T : DataProvider, new()
        {
            connection = connection ?? throw new ArgumentNullException(nameof(connection));
            var r = new T();
            r.init(connection);
            return r;
        }

        public static T Create<T>(AppServiceConnection connection, Func<T> factory)
            where T : DataProvider
        {
            if (factory is null)
                throw new ArgumentNullException(nameof(factory));
            connection = connection ?? throw new ArgumentNullException(nameof(connection));
            var r = factory();
            r.init(connection);
            return r;
        }

        protected DataProvider() { }

        private AppServiceConnection connection;
        private void init(AppServiceConnection connection)
        {
            this.connection = connection;
            this.connection.RequestReceived += this.Connection_RequestReceived;
        }

        private async void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var def = args.GetDeferral();
            try
            {
                IResponseMessage response;
                switch (args.Request.GetMessage())
                {
                case SearchRequest search:
                    response = await SearchAsync(search);
                    break;
                case GetBookRequest getBook:
                    response = await GetBookAsync(getBook);
                    break;
                case GetChaptersRequest getChapters:
                    response = await GetChaptersAsync(getChapters);
                    break;
                default:
                    response = ErrorResponse.NotImplemented;
                    break;
                }
                await args.Request.SendResponseAsync(response);
            }
            catch (Exception ex)
            {
                Debugger.Break();
                await args.Request.SendResponseAsync(new ErrorResponse(ex.HResult, ex.Message, ex));
            }
            finally
            {
                def.Complete();
            }
        }

        protected abstract Task<SearchResponse> SearchAsync(SearchRequest request);
        protected abstract Task<GetBookResponse> GetBookAsync(GetBookRequest request);
        protected abstract Task<GetChaptersResponse> GetChaptersAsync(GetChaptersRequest request);
    }
}
