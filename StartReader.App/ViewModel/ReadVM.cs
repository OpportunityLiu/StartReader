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
                return new ReadVM(bookId, chpIdx);
            });
        }

        public ReadVM(int bookId, int chapterIndex)
        {
            this.BookId = bookId;
            this.ChapterIndex = chapterIndex;
            using (var bs = BookShelf.Create())
            {
                bs.Chapters.Include(c => c.Book).Include(c => c.Source)
                    .Where(c => c.BookId == bookId && c.Index >= chapterIndex - 1 && c.Index <= chapterIndex + 1).Load();
                this.chapter = bs.Chapters.Find(bookId, chapterIndex);
                this.previous = bs.Chapters.Find(bookId, chapterIndex - 1);
                this.next = bs.Chapters.Find(bookId, chapterIndex + 1);
            }
            if (this.chapter.Content.IsNullOrEmpty() ||
                (this.next != null && this.next.Content.IsNullOrEmpty()) ||
                (this.previous != null && this.previous.Content.IsNullOrEmpty()))
                Refresh.Execute(false);
        }

        private double position;
        public double Position
        {
            get => this.position;
            set
            {
                if (double.IsNaN(value) || value < 0)
                    value = 0;
                else if (value > 1)
                    value = 1;
                Set(ref this.position, value);
            }
        }

        public int BookId { get; }
        public int ChapterIndex { get; }

        private Chapter chapter;
        public Chapter Chapter { get => this.chapter; set => Set(ref this.chapter, value); }

        private Chapter previous;
        public Chapter Previous { get => this.previous; set => Set(ref this.previous, value); }

        private Chapter next;
        public Chapter Next { get => this.next; set => Set(ref this.next, value); }

        public AsyncCommand<bool> Refresh => Commands.GetOrAdd(() => AsyncCommand<bool>.Create(async (s, force) =>
        {
            using (var bs = BookShelf.Create())
            {
                var chps = bs.Chapters.Include(c => c.Book).Include(c => c.Source)
                    .Where(c => c.BookId == this.BookId && c.Index >= this.ChapterIndex - 1 && c.Index <= this.ChapterIndex + 3)
                    .OrderBy(c => c.Index)
                    .ToList();
                var chapter = bs.Chapters.Find(this.BookId, this.ChapterIndex);

                var qchpsToLoad = chps.Where(c => c.SourceId == chapter.SourceId);
                if (!force)
                    qchpsToLoad = qchpsToLoad.Where(c => c.Content.IsNullOrEmpty());
                var chpsToLoad = qchpsToLoad.ToList();

                if (chpsToLoad.IsNullOrEmpty())
                    return;

                var detail = (await this.Chapter.Source.FindSource().ExecuteAsync(new GetChaptersRequest
                {
                    BookKey = chapter.Source.BookKey,
                    ChapterKeys = chpsToLoad.Select(c => c.Key).ToList(),
                }));
                for (var i = 0; i < detail.Chapters.Count; i++)
                {
                    bs.Entry(chpsToLoad[i]).CurrentValues.SetValues(detail.Chapters[i]);
                }
                bs.SaveChanges();

                this.Chapter = chapter;
                this.Previous = bs.Chapters.Find(this.BookId, this.ChapterIndex - 1);
                this.Next = bs.Chapters.Find(this.BookId, this.ChapterIndex + 1);
            }
        }));

        public Command<Chapter> GoToChapter => Commands.GetOrAdd(() => Command<Chapter>.Create(async (s, ch) =>
        {
            if (ch == this.previous)
                ViewModelFactory.Get<ReadVM>(ch.BookId + " " + ch.Index).Position = 1;
            else if (ch == this.next)
                ViewModelFactory.Get<ReadVM>(ch.BookId + " " + ch.Index).Position = 0;
            await Navigator.GetForCurrentView().NavigateAsync(typeof(ReadPage), ch.BookId + " " + ch.Index);
        }, (s, ch) => ch != null));
    }
}
