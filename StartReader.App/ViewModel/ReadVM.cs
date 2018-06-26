using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Opportunity.MvvmUniverse.Commands;
using Opportunity.MvvmUniverse.Services.Navigation;
using Opportunity.MvvmUniverse.Views;
using StartReader.App.Model;
using StartReader.App.View;
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
                var bookId = int.Parse(ss[0]);
                var chpIdx = int.Parse(ss[1]);
                var pos = ss.Length >= 3 ? double.Parse(ss[2]) : 0;
                return new ReadVM(bookId, chpIdx, pos);
            });
        }

        public ReadVM(int bookId, int chapterIndex, double position)
        {
            this.BookId = bookId;
            this.ChapterIndex = chapterIndex;
            this.position = position;
            using (var bs = BookShelf.Create())
            {
                bs.Chapters.Include(c => c.Book).Include(c => c.Source)
                    .Where(c => c.BookId == bookId && c.Index >= chapterIndex - 1 && c.Index <= chapterIndex + 1).Load();
                this.chapter = bs.Chapters.Find(bookId, chapterIndex);
                this.previous = bs.Chapters.Find(bookId, chapterIndex - 1);
                this.next = bs.Chapters.Find(bookId, chapterIndex + 1);
            }
            if (this.chapter.Content.IsNullOrEmpty())
                Load.Execute();
        }

        private double position;
        public double Position { get => this.position; set => Set(ref this.position, value); }

        public int BookId { get; }
        public int ChapterIndex { get; }

        private Chapter chapter;
        public Chapter Chapter { get => this.chapter; set => Set(ref this.chapter, value); }

        private Chapter previous;
        public Chapter Previous { get => this.previous; set => Set(ref this.previous, value); }

        private Chapter next;
        public Chapter Next { get => this.next; set => Set(ref this.next, value); }

        public AsyncCommand Refresh => Commands.GetOrAdd(() => AsyncCommand.Create(async s =>
        {
            using (var bs = BookShelf.Create())
            {
                bs.Chapters.Include(c => c.Book).Include(c => c.Source)
                    .Where(c => c.BookId == BookId && c.Index >= ChapterIndex - 1 && c.Index <= ChapterIndex + 1).Load();
                var chapter = bs.Chapters.Find(BookId, ChapterIndex);
                var previous = bs.Chapters.Find(BookId, ChapterIndex - 1);
                var next = bs.Chapters.Find(BookId, ChapterIndex + 1);
                var detail = (await Chapter.Source.FindSource().ExecuteAsync(new GetChaptersRequest
                {
                    BookKey = Chapter.Source.BookKey,
                    ChapterKeys = new[] { Chapter.Key },
                })).Chapters[0];
                bs.Entry(chapter).CurrentValues.SetValues(detail);
                bs.SaveChanges();
                this.Chapter = chapter;
                this.Previous = previous;
                this.Next = next;
            }
        }));

        public AsyncCommand Load => Commands.GetOrAdd(() => AsyncCommand.Create(async s =>
        {
            using (var bs = BookShelf.Create())
            {
                var sourceId = this.chapter.SourceId;
                var chps = (from c in bs.Chapters
                            where c.Index >= ChapterIndex && string.IsNullOrEmpty(c.Content) && c.SourceId == sourceId
                            orderby c.Index
                            select c).Take(5).ToList();
                var details = (await Chapter.Source.FindSource().ExecuteAsync(new GetChaptersRequest
                {
                    BookKey = Chapter.Source.BookKey,
                    ChapterKeys = chps.Select(c => c.Key).ToList(),
                })).Chapters;
                for (var i = 0; i < details.Count; i++)
                {
                    bs.Entry(chps[i]).CurrentValues.SetValues(details[i]);
                }
                bs.SaveChanges();
                var ch = chps.Find(c => c.Index == ChapterIndex);
                if (ch != null)
                    Chapter = ch;
                var nch = chps.Find(c => c.Index == ChapterIndex + 1);
                if (nch != null)
                    Next = nch;
            }
        }));

        public Command<Chapter> GoToChapter => Commands.GetOrAdd(() => Command<Chapter>.Create(async (s, ch) =>
        {
            await Navigator.GetForCurrentView().NavigateAsync(typeof(ReadPage), ch.BookId + " " + ch.Index);
        }, (s, ch) => ch != null));
    }
}
