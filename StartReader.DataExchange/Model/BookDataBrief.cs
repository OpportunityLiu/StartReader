using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace StartReader.DataExchange.Model
{
    public class BookDataBrief
    {
        [JsonRequired]
        public string Key { get; set; }
        [JsonRequired]
        public string Title { get; set; }
        public string AlternativeTitle { get; set; }
        [JsonRequired]
        public string Author { get; set; }
        public string Description { get; set; }

        public ChapterDataBrief LastestChapter { get; set; }
    }
}
