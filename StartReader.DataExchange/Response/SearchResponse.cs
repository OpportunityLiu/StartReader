using Newtonsoft.Json;
using StartReader.DataExchange.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartReader.DataExchange.Response
{
    public class BookDataBrief
    {
        [JsonRequired]
        public string Key;
        [JsonRequired]
        public string Title;
        public string AlternativeTitle;
        [JsonRequired]
        public string Author;
        public string Description;
        [JsonRequired]
        public Uri Url;

        public ChapterDataBrief LastestChapter;
    }

    public class BookDataDetailed : BookDataBrief
    {
        [JsonRequired]
        public IList<ChapterDataBrief> Chapters { get; } = new List<ChapterDataBrief>();
    }

    public class ChapterDataBrief
    {
        [JsonRequired]
        public string Key;
        [JsonRequired]
        public string BookKey;
        [JsonRequired]
        public string Title;
        public string Preview;
    }

    public class ChapterDataDetailed : ChapterDataBrief
    {
        [JsonRequired]
        public string Content;
    }

    public sealed class SearchResponse : IResponseMessage<SearchRequest, SearchResponse>
    {
        [JsonRequired]
        public IList<BookDataBrief> Books { get; } = new List<BookDataBrief>();
    }
}
