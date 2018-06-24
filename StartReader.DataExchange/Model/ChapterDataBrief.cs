using Newtonsoft.Json;

namespace StartReader.DataExchange.Model
{
    public class ChapterDataBrief
    {
        [JsonRequired]
        public string Key { get; set; }
        [JsonRequired]
        public string BookKey { get; set; }
        [JsonRequired]
        public string Title { get; set; }
        public string Preview { get; set; }
    }
}
