using Newtonsoft.Json;
using System.Collections.Generic;
using StartReader.DataExchange.Response;

namespace StartReader.DataExchange.Request
{
    public sealed class GetChaptersRequest : RequestMessageBase, IRequestMessage<GetChaptersRequest, GetChaptersResponse>
    {
        string IRequestMessage.Method => "GetChapters";

        [JsonRequired]
        public string BookKey { get; internal set; }

        [JsonRequired]
        public IReadOnlyList<string> ChapterKeys { get; internal set; }
    }
}
