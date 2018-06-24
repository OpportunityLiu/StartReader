using Newtonsoft.Json;

namespace StartReader.DataExchange.Model
{
    public class ChapterDataDetailed : ChapterDataBrief
    {
        [JsonRequired]
        public string Content { get; set; }
    }
}
