using System.Threading.Tasks;
using StartReader.DataExchange;
using StartReader.DataExchange.Request;
using StartReader.DataExchange.Response;
using Windows.ApplicationModel.AppService;

namespace StartReader.ExtensionProvider
{
    internal class MiaoBiGeProvider : DataExchangeProvider
    {
        public MiaoBiGeProvider(AppServiceConnection connection) : base(connection)
        {
        }

        protected override Task<GetBookResponse> GetBook(GetBookRequest request) => throw new System.NotImplementedException();
        protected override Task<SearchResponse> Search(SearchRequest request) => throw new System.NotImplementedException();
    }
}