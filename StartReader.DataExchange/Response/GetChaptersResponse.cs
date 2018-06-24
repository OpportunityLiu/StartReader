using Newtonsoft.Json;
using StartReader.DataExchange.Model;
using StartReader.DataExchange.Request;

namespace StartReader.DataExchange.Response
{
    public sealed class GetChaptersResponse : IResponseMessage<GetChaptersRequest, GetChaptersResponse>
    {
        [JsonRequired]
        public BookDataDetailed BookData { get; set; }
    }
}
