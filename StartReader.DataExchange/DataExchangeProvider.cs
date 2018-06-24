using StartReader.DataExchange.Response;
using System;
using Windows.ApplicationModel.AppService;

namespace StartReader.DataExchange
{
    public abstract class DataExchangeProvider
    {
        private readonly AppServiceConnection connection;

        public DataExchangeProvider(AppServiceConnection connection)
        {
            this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
            this.connection.RequestReceived += this.Connection_RequestReceived;
            this.connection.ServiceClosed += this.Connection_ServiceClosed;
        }

        private void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {

        }

        private async void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var def = args.GetDeferral();
            await args.Request.SendResponseAsync(new ErrorResponse(-1, "nono"));
            def.Complete();
        }
    }
}
