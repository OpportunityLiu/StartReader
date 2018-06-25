using Newtonsoft.Json;
using StartReader.DataExchange.Model;
using StartReader.DataExchange.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartReader.DataExchange.Response
{
    public sealed class GetBookResponse : ResponseMessageBase,IResponseMessage<GetBookRequest, GetBookResponse>
    {
        [JsonProperty(Required = Required.Always)]
        public BookDataDetailed BookData { get; set; }
    }
}
