using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Opportunity.MvvmUniverse.Collections;
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
                this._Book = bs.Books.Find(bookId);
                bs.Chapters.Where(c => c.BookId == bookId).OrderBy(c => c.ChapterId).Load();
                this._BookView = this._Book.ChaptersData.CreateView();
                this._BookView.CurrentChanged += this._BookView_CurrentChanged;
            }
        }

        private void _BookView_CurrentChanged(object sender, object e)
        {
            var args = (CurrentChangedEventArgs<Chapter>)e;
            var chapterIndex = this._BookView.CurrentPosition;
            var current = this._BookView.CurrentItem;
            var next = chapterIndex + 1 < this._BookView.Count ? this._BookView[chapterIndex + 1] : null;
            var previous = chapterIndex - 1 > 0 ? this._BookView[chapterIndex - 1] : null;
            if (args.OldPosition - chapterIndex == 1)
                this.Position = 1;
            else
                this.Position = 0;
            if (string.IsNullOrEmpty(current?.Content) ||
                string.IsNullOrEmpty(next?.Content) ||
                string.IsNullOrEmpty(previous?.Content))
                this.Refresh.Execute(false);
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

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Book _Book;
        public Book Book { get => this._Book; private set => Set(ref this._Book, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private CollectionView<Chapter> _BookView;
        public CollectionView<Chapter> BookView { get => this._BookView; private set => Set(ref this._BookView, value); }

        public AsyncCommand<bool> Refresh => Commands.GetOrAdd(() => AsyncCommand<bool>.Create(async (s, force) =>
        {
            var current = this._BookView.CurrentItem;
            if (current is null)
                return;
            using (var bs = BookShelf.Create())
            {
                bs.Attach(this._Book);
                var chapterId = current.ChapterId;
                var chps = await bs.Chapters.Include(c => c.Book)
                    .Where(c => c.BookId == this._Book.Id && c.ChapterId >= chapterId - 1 && c.ChapterId <= chapterId + 3)
                    .OrderBy(c => c.ChapterId)
                    .ToListAsync();
                current = bs.Chapters.Find(this._Book.Id, chapterId);

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
                    chpsToLoad[i].OnObjectReset();
                }
                await bs.SaveChangesAsync();
            }
        }));
    }
}
