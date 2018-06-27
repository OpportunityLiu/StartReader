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
                var bookId = int.Parse(s);
                return new ReadVM(bookId);
            });
        }

        private ReadVM(int bookId)
        {
            using (var bs = BookShelf.Create())
            {
                this.Book = bs.Books.Find(bookId);
            }
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

        public Book Book { get; }

        private Chapter current;
        public Chapter Current { get => this.current; set => Set(ref this.current, value); }

        private Chapter previous;
        public Chapter Previous { get => this.previous; set => Set(ref this.previous, value); }

        private Chapter next;
        public Chapter Next { get => this.next; set => Set(ref this.next, value); }

        public AsyncCommand<bool> Refresh => Commands.GetOrAdd(() => AsyncCommand<bool>.Create(async (s, force) =>
        {
            var current = this.current;
            if (current is null)
                return;
            using (var bs = BookShelf.Create())
            {
                var bookId = this.Book.Id;
                var chapterIndex = current.Index;
                var chps = bs.Chapters.Include(c => c.Book).Include(c => c.Source)
                    .Where(c => c.BookId == bookId && c.Index >= chapterIndex - 1 && c.Index <= chapterIndex + 3)
                    .OrderBy(c => c.Index)
                    .ToList();
                current = bs.Chapters.Find(bookId, chapterIndex);

                var qchpsToLoad = chps.Where(c => c.SourceId == current.SourceId);
                if (!force)
                    qchpsToLoad = qchpsToLoad.Where(c => c.Content.IsNullOrEmpty());
                var chpsToLoad = qchpsToLoad.ToList();

                if (chpsToLoad.IsNullOrEmpty())
                    return;

                var detail = (await this.Current.Source.FindSource().ExecuteAsync(new GetChaptersRequest
                {
                    BookKey = current.Source.BookKey,
                    ChapterKeys = chpsToLoad.Select(c => c.Key).ToList(),
                }));
                for (var i = 0; i < detail.Chapters.Count; i++)
                {
                    bs.Entry(chpsToLoad[i]).CurrentValues.SetValues(detail.Chapters[i]);
                }
                bs.SaveChanges();

                this.Current = current;
                this.Previous = bs.Chapters.Find(bookId, chapterIndex - 1);
                this.Next = bs.Chapters.Find(bookId, chapterIndex + 1);
            }
        }));

        public Command<int> GoToChapter => Commands.GetOrAdd(() => Command<int>.Create((s, chapterIndex) =>
        {
            using (var bs = BookShelf.Create())
            {
                var bookId = this.Book.Id;
                bs.Chapters.Include(c => c.Book).Include(c => c.Source)
                    .Where(c => c.BookId == bookId && c.Index >= chapterIndex - 1 && c.Index <= chapterIndex + 1).Load();
                if (chapterIndex == this.previous?.Index)
                    this.Position = 1;
                else
                    this.Position = 0;
                this.Current = bs.Chapters.Find(bookId, chapterIndex);
                this.Previous = bs.Chapters.Find(bookId, chapterIndex - 1);
                this.Next = bs.Chapters.Find(bookId, chapterIndex + 1);
            }
            if (this.current.Content.IsNullOrEmpty() ||
                (this.next != null && this.next.Content.IsNullOrEmpty()) ||
                (this.previous != null && this.previous.Content.IsNullOrEmpty()))
                this.Refresh.Execute(false);
        }, (s, chapterIndex) => chapterIndex >= 0));
    }
}
