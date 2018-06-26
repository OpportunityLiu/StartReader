using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StartReader.DataExchange.Model
{
    public class ChapterDataBrief : ModelBase
    {
        [JsonProperty(Required = Required.Always)]
        [Required]
        public string Title { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public DateTime? UpdateTime { get; set; }

        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int WordCount { get; set; } = -1;

        [JsonProperty(Required = Required.Always)]
        [NotMapped]
        public string Key { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [NotMapped]
        public string Preview { get; set; }
    }

    public class ChapterDataDetailed : ChapterDataBrief
    {
        [JsonProperty(Required = Required.Always)]
        public string Content { get; set; }
    }
}
