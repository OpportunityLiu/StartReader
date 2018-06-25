using Newtonsoft.Json;
using StartReader.DataExchange.Model;
using StartReader.DataExchange.Request;
using System.Collections;
using System.Collections.Generic;

namespace StartReader.DataExchange.Response
{
    public sealed class GetChaptersResponse : ResponseMessageBase, IResponseMessage<GetChaptersRequest, GetChaptersResponse>
    {
        [JsonRequired]
        public IList<ChapterDataDetailed> Chapters { get; } = new List<ChapterDataDetailed>();
    }
}
