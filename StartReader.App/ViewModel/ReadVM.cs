using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Opportunity.MvvmUniverse.Views;
using StartReader.App.Model;
using StartReader.DataExchange.Model;
using StartReader.DataExchange.Request;

namespace StartReader.App.ViewModel
{
    class ReadVM : ViewModelBase
    {
        static ReadVM()
        {
            ViewModelFactory.Register(s => new ReadVM(JsonConvert.DeserializeObject<Chapter>(s)));
        }

        public ReadVM(Chapter chapter)
        {
            this.Chapter = chapter;
            Task.Run(async () =>
            {
                this.Detailed = (await Chapter.Source.FindSource().ExecuteAsync(new GetChaptersRequest
                {
                    BookKey = Chapter.Source.BookKey,
                    ChapterKeys = new[] { Chapter.Key },
                })).Chapters[0];
            });
        }

        public Chapter Chapter { get; }

        private ChapterDataDetailed detailed;
        public ChapterDataDetailed Detailed { get => this.detailed; set => Set(ref this.detailed, value); }

    }
}
