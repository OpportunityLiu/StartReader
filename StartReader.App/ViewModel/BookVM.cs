using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Opportunity.Helpers.Universal.AsyncHelpers;
using Opportunity.MvvmUniverse.Collections;
using Opportunity.MvvmUniverse.Commands;
using Opportunity.MvvmUniverse.Services.Navigation;
using Opportunity.MvvmUniverse.Views;
using StartReader.App.Extensiton;
using StartReader.App.Model;
using StartReader.App.View;
using StartReader.DataExchange.Model;
using StartReader.DataExchange.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace StartReader.App.ViewModel
{
    class BookVM : ViewModelBase
    {
        static BookVM()
        {
            ViewModelFactory.Register(s => new BookVM(int.Parse(s)));
        }

        private BookVM(int bookId)
        {
            using (var bs = BookShelf.Create())
            {
                Book = bs.Books.Find(bookId);
                bs.Entry(Book).Collection(b => b.ChaptersData).Query().OrderBy(c => c.Index).Load();
            }
            if (Book.ChaptersData.IsEmpty())
                Refersh.Execute();
        }

        private Book book;
        public Book Book
        {
            get => this.book;
            set
            {
                Set(ref this.book, value);
                FetchAll.OnCanExecuteChanged();
            }
        }

        public AsyncCommand Refersh => Commands.GetOrAdd(() => AsyncCommand.Create(async sender =>
        {
            using (var bs = BookShelf.Create())
            {
                var book = bs.Books.Find(this.book.Id);
                bs.Entry(book).Collection(b => b.ChaptersData).Query().OrderBy(c => c.Index).Load();
                var data = await book.FindSource().ExecuteAsync(new GetBookRequest { BookKey = book.Key, NeedDetail = true });
                book.Update(data.BookData, data.Source);
                await bs.SaveChangesAsync();
                this.Book = book;
            }
        }));

        public AsyncCommandWithProgress<int> FetchAll
            => Commands.GetOrAdd(() => AsyncCommandWithProgress<int>.Create(sender => AsyncInfo.Run<int>(async (token, progress) =>
        {
            using (var bs = BookShelf.Create())
            {
                var book = bs.Books.Find(this.book.Id);
                bs.Entry(book).Collection(b => b.ChaptersData).Query().OrderBy(c => c.Index).Load();
                try
                {
                    var chpsToLoad = book.ChaptersData.Where(ch => ch.Content.IsNullOrEmpty()).ToList();
                    if (chpsToLoad.IsEmpty())
                        return;

                    progress.Report(book.ChaptersData.Count - chpsToLoad.Count);
                    await Task.Delay(0).ConfigureAwait(false);

                    const int PAGE_SIZE = 5;

                    for (var offset = 0; offset < chpsToLoad.Count; offset += PAGE_SIZE)
                    {
                        var data = await book.FindSource().ExecuteAsync(new GetChaptersRequest
                        {
                            BookKey = book.Key,
                            ChapterKeys = chpsToLoad.Skip(offset).Take(PAGE_SIZE).Select(ch => ch.Key).ToList(),
                        }, token);
                        for (var i = 0; i < data.Chapters.Count; i++)
                        {
                            chpsToLoad[i + offset].Update(data.Chapters[i]);
                        }
                        await bs.SaveChangesAsync();
                        progress.Report(book.ChaptersData.Count - chpsToLoad.Count + offset);
                    }
                }
                finally
                {
                    this.Book = book;
                }
            }
        }), sender => !this.book.ChaptersData.IsEmpty()));

        public async void Open(Chapter chapter)
        {
            var vm = ViewModelFactory.Get<ReadVM>(this.book.Id.ToString());
            vm.GoToChapter.Execute(-1);
            vm.GoToChapter.Execute(chapter.Index);
            await Navigator.GetForCurrentView().NavigateAsync(typeof(ReadPage), chapter.Book.Id);
        }
    }
}
