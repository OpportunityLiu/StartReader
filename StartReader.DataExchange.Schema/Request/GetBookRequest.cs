using Newtonsoft.Json;
using StartReader.DataExchange.Response;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartReader.DataExchange.Request
{
    public sealed class GetBookRequest : RequestMessageBase, IRequestMessage<GetBookRequest, GetBookResponse>
    {
        string IRequestMessage.Method => "GetBook";

        [JsonRequired]
        public string BookKey { get; internal set; }

        [JsonRequired]
        public bool NeedDetail { get; internal set; } 
    }
}
