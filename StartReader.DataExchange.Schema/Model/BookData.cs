using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StartReader.DataExchange.Model
{
    public class BookDataBrief
    {
        [JsonProperty(Required = Required.Always)]
        [NotMapped]
        public string Key { get; set; }

        [JsonProperty(Required = Required.Always)]
        [Required]
        public string Title { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string AlternativeTitle { get; set; }

        [JsonProperty(Required = Required.Always)]
        [Required]
        public string Author { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Description { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public byte[] CoverData { get; set; }

        private string coverUri;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [NotMapped]
        public Uri CoverUri
        {
            get => string.IsNullOrWhiteSpace(this.coverUri) ? null : new Uri(this.coverUri);
            set => this.coverUri = value?.ToString();
        }

        public bool IsFinished { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [NotMapped]
        public ChapterDataBrief LastestChapter { get; set; }

        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int WordCount { get; set; } = -1;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [NotMapped]
        public string[] Tags { get; set; }
    }

    public class BookDataDetailed : BookDataBrief
    {
        [NotMapped]
        public IList<ChapterDataBrief> Chapters { get; } = new List<ChapterDataBrief>();
    }
}
