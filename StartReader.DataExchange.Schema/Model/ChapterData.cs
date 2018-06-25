using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace StartReader.DataExchange.Model
{
    public class ChapterDataBrief
    {
        [Required]
        public string Key { get; set; }
        [Required]
        public string Title { get; set; }

        public DateTime? UpdateTime { get; set; }

        public string Preview { get; set; }

        public int WordCount { get; set; } = -1;
    }

    public class ChapterDataDetailed : ChapterDataBrief
    {
        [Required]
        public string Content { get; set; }
    }
}
