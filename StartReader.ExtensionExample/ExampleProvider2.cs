﻿using StartReader.DataExchange;
using StartReader.DataExchange.Request;
using StartReader.DataExchange.Response;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;

namespace StartReader.ExtensionExample
{
    public sealed class ExampleProvider2 : DataExchangeProvider
    {
        public ExampleProvider2(AppServiceConnection connection) : base(connection)
        {
        }

        protected override Task<GetBookResponse> GetBookAsync(GetBookRequest request) => throw new NotImplementedException();
        protected override Task<GetChaptersResponse> GetChaptersAsync(GetChaptersRequest request) => throw new NotImplementedException();
        protected override Task<SearchResponse> SearchAsync(SearchRequest request) => throw new NotImplementedException();
    }
}
