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
            this.BookId = bookId;
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
                var chapterIndex = current.Index;
                var chps = await bs.Chapters.Include(c => c.Book)
                    .Where(c => c.BookId == BookId && c.Index >= chapterIndex - 1 && c.Index <= chapterIndex + 3)
                    .OrderBy(c => c.Index)
                    .ToListAsync();
                current = bs.Chapters.Find(BookId, chapterIndex);

                var chpsToLoad = force ? chps : chps.Where(c => c.Content.IsNullOrEmpty()).ToList();

                if (chpsToLoad.IsNullOrEmpty())
                    return;

                var detail = (await current.Book.FindSource().ExecuteAsync(new GetChaptersRequest
                {
                    BookKey = current.Book.Key,
                    ChapterKeys = chpsToLoad.Select(c => c.Key).ToList(),
                }));
                for (var i = 0; i < detail.Chapters.Count; i++)
                {
                    chpsToLoad[i].Update(detail.Chapters[i]);
                }
                await bs.SaveChangesAsync();

                this.Current = current;
                this.Previous = bs.Chapters.Find(BookId, chapterIndex - 1);
                this.Next = bs.Chapters.Find(BookId, chapterIndex + 1);
            }
        }));

        public Command<int> GoToChapter => Commands.GetOrAdd(() => Command<int>.Create((s, chapterIndex) =>
        {
            if (chapterIndex < 0)
            {
                this.Current = null;
                this.Previous = null;
                this.Next = null;
                return;
            }
            using (var bs = BookShelf.Create())
            {
                bs.Chapters.Include(c => c.Book)
                    .Where(c => c.BookId == BookId && c.Index >= chapterIndex - 1 && c.Index <= chapterIndex + 1).Load();
                if (chapterIndex == this.previous?.Index)
                    this.Position = 1;
                else
                    this.Position = 0;
                this.Current = bs.Chapters.Find(BookId, chapterIndex);
                this.Previous = bs.Chapters.Find(BookId, chapterIndex - 1);
                this.Next = bs.Chapters.Find(BookId, chapterIndex + 1);
            }
            if (this.current.Content.IsNullOrEmpty() ||
                (this.next != null && this.next.Content.IsNullOrEmpty()) ||
                (this.previous != null && this.previous.Content.IsNullOrEmpty()))
                this.Refresh.Execute(false);
        }));
    }
}
