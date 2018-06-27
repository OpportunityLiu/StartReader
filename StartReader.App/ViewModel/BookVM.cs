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
                Book = bs.Books.Include(b => b.Sources).First(b => b.Id == bookId);
                bs.Entry(Book).Collection(b => b.ChaptersData).Query().OrderBy(c => c.Index).Load();
            }
            if (Book.ChaptersData.IsNullOrEmpty())
                Refersh.Execute(null);
        }

        private Book book;
        public Book Book { get => this.book; set => Set(ref this.book, value); }

        public AsyncCommand<int?> Refersh => Commands.GetOrAdd(() => AsyncCommand<int?>.Create(async (sender, sourceId) =>
        {
            using (var bs = BookShelf.Create())
            {
                var book = bs.Books.Find(this.book.Id);
                bs.Entry(book).Collection(b => b.ChaptersData).Query().OrderBy(c => c.Index).Load();
                bs.Entry(book).Collection(b => b.Sources).Load();

                var oldCurrentSource = book.Sources.First(s => s.IsCurrent);
                sourceId = sourceId ?? oldCurrentSource.Id;
                var newCurrentSource = book.Sources.First(s => s.Id == sourceId);
                if (oldCurrentSource != newCurrentSource)
                {
                    oldCurrentSource.IsCurrent = false;
                    newCurrentSource.IsCurrent = true;
                    book.ChaptersData.Clear();
                    await bs.SaveChangesAsync();
                }

                var data = await newCurrentSource.FindSource().ExecuteAsync(new GetBookRequest { BookKey = newCurrentSource.BookKey });
                JsonConvert.PopulateObject(JsonConvert.SerializeObject(data.BookData), book);
                var newChpData = new List<Chapter>();
                var i = 0;
                foreach (var item in book.Chapters)
                {
                    newChpData.Add(new Chapter
                    {
                        Index = i,
                        Key = item.Key,
                        Book = this.book,
                        Preview = item.Preview,
                        Source = newCurrentSource,
                        UpdateTime = item.UpdateTime,
                        Title = item.Title,
                        WordCount = item.WordCount,
                    });
                    i++;
                }
                book.ChaptersData.Update(newChpData, (o, n) => o.Index.CompareTo(n.Index), (existChp, newChp) =>
                {
                    existChp.Key = newChp.Key;
                    existChp.Source = newChp.Source;
                    existChp.Title = newChp.Title;
                    existChp.UpdateTime = newChp.UpdateTime ?? existChp.UpdateTime;
                    if (newChp.WordCount >= 0)
                        existChp.WordCount = newChp.WordCount;
                });
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
                bs.Entry(book).Collection(b => b.Sources).Load();

                var source = bs.BookSources.Single(s => s.BookId == this.book.Id && s.IsCurrent);
                var chpsToLoad = book.ChaptersData.Where(ch => ch.Content.IsNullOrEmpty() && ch.SourceId == source.Id).ToList();
                progress.Report(book.ChaptersData.Count - chpsToLoad.Count);
                try
                {
                    for (var offset = 0; offset < chpsToLoad.Count; offset += 10)
                    {
                        var data = await source.FindSource().ExecuteAsync(new GetChaptersRequest
                        {
                            BookKey = source.BookKey,
                            ChapterKeys = chpsToLoad.Skip(offset).Take(10).Select(ch => ch.Key).ToList(),
                        }, token);
                        for (var i = 0; i < data.Chapters.Count; i++)
                        {
                            var existCh = chpsToLoad[i + offset];
                            var newCh = data.Chapters[i];
                            existCh.Key = newCh.Key;
                            existCh.Content = newCh.Content;
                            existCh.WordCount = newCh.WordCount;
                            existCh.Title = newCh.Title;
                            existCh.UpdateTime = newCh.UpdateTime;
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
        })));

        public async void Open(Chapter chapter)
        {
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(ReadVM).TypeHandle);
            ViewModelFactory.Get<ReadVM>(this.book.Id.ToString()).GoToChapter.Execute(chapter.Index);
            await Navigator.GetForCurrentView().NavigateAsync(typeof(ReadPage), chapter.Book.Id);
        }

    }
}
