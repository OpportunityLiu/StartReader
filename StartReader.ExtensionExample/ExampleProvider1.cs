using StartReader.DataExchange;
using StartReader.DataExchange.Request;
using StartReader.DataExchange.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;

namespace StartReader.ExtensionExample
{
    public sealed class ExampleProvider1 : DataExchangeProvider
    {
        public ExampleProvider1(AppServiceConnection connection) : base(connection)
        {
        }

        protected override Task<GetBookResponse> GetBook(GetBookRequest request) => throw new NotImplementedException();
        protected override Task<SearchResponse> Search(SearchRequest request) => throw new NotImplementedException();
    }
}
