using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            ViewModelFactory.Register(s =>
            {
                var ss = s.Split();
                return new ReadVM(int.Parse(ss[0]), int.Parse(ss[1]));
            });
        }

        public ReadVM(int bookId, int chapterIndex)
        {
            Task.Run(async () =>
            {
                using (var bs = BookShelf.Create())
                {
                    this.Chapter = bs.Chapters.Include(c => c.Book).First(c => c.Book.Id == bookId && c.Index == chapterIndex);
                    var detail = (await Chapter.Source.FindSource().ExecuteAsync(new GetChaptersRequest
                    {
                        BookKey = Chapter.Source.BookKey,
                        ChapterKeys = new[] { Chapter.Key },
                    })).Chapters[0];
                    bs.Entry(this.chapter).CurrentValues.SetValues(detail);
                    bs.SaveChanges();
                }
            });
        }

        private Chapter chapter;
        public Chapter Chapter { get => this.chapter; set => Set(ref this.chapter, value); }

    }
}
