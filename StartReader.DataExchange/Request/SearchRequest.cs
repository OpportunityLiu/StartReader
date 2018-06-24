using Newtonsoft.Json;
using StartReader.DataExchange.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartReader.DataExchange.Request
{
    public sealed class SearchRequest : RequestMessageBase, IRequestMessage<SearchRequest, SearchResponse>
    {
        string IRequestMessage.Method => "Search";

        [JsonRequired]
        public string Keyword { get; set; }
    }
}
