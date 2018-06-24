using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace StartReader.DataExchange.Model
{
    public class BookDataDetailed : BookDataBrief
    {
        public byte[] Cover { get; set; }

        [JsonRequired]
        public IList<ChapterDataBrief> Chapters { get; } = new List<ChapterDataBrief>();
    }
}
