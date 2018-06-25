using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StartReader.DataExchange.Model
{
    public class BookDataOutline
    {
        [Required]
        public string Title { get; set; }

        public string AlternativeTitle { get; set; }
        [Required]
        public string Author { get; set; }

        public string Description { get; set; }

        public byte[] CoverData { get; set; }

        private string coverUri;
        public Uri CoverUri { get => new Uri(this.coverUri); set => this.coverUri = value?.ToString(); }

        public bool IsFinished { get; set; }
    }

    public class BookDataBrief : BookDataOutline
    {
        [Required]
        public string Key { get; set; }

        public ChapterDataBrief LastestChapter { get; set; }

        public int WordCount { get; set; } = -1;

        public string[] Tags { get; set; } = Array.Empty<string>();
    }

    public class BookDataDetailed : BookDataBrief
    {
        [JsonRequired]
        public IList<ChapterDataBrief> Chapters { get; } = new List<ChapterDataBrief>();
    }
}
