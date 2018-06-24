﻿using StartReader.DataExchange.Request;
using StartReader.DataExchange.Response;
using System;
using System.Threading.Tasks;
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
                    response = await Search(search);
                    break;
                case GetBookRequest getBook:
                    response = await GetBook(getBook);
                    break;
                default:
                    response = ErrorResponse.NotImplemented;
                    break;
                }
                await args.Request.SendResponseAsync(response);
            }
            catch (NotImplementedException)
            {
                await args.Request.SendResponseAsync(ErrorResponse.NotImplemented);
            }
            catch (Exception ex)
            {
                await args.Request.SendResponseAsync(new ErrorResponse(ex.HResult, ex.Message));
            }
            finally
            {
                def.Complete();
            }
        }

        protected abstract Task<SearchResponse> Search(SearchRequest request);
        protected abstract Task<GetBookResponse> GetBook(GetBookRequest request);
    }
}
