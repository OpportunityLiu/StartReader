using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Opportunity.MvvmUniverse.Collections;
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
            Task.Run(async () =>
            {
                using (var bs = BookShelf.Create())
                {
                    this.Book = bs.Books.Include(b => b.Sources).Include(b => b.ChaptersData).First(b => b.Id == bookId);
                    var source = this.book.Sources.First(s => s.IsCurrent);
                    var data = await source.FindSource().ExecuteAsync(new GetBookRequest { BookKey = source.BookKey });
                    JsonConvert.PopulateObject(JsonConvert.SerializeObject(data.BookData), this.book);
                    var newChpData = new List<Chapter>();
                    var i = 0;
                    foreach (var item in this.book.Chapters)
                    {
                        newChpData.Add(new Chapter
                        {
                            Index = i,
                            Book = this.book,
                            Preview = item.Preview,
                            Source = source,
                            UpdateTime = item.UpdateTime,
                            Title = item.Title,
                            WordCount = item.WordCount,
                        });
                        i++;
                    }
                    this.book.ChaptersData.Update(newChpData, (o, n) => o.Index.CompareTo(n.Index), (existChp, newChp) =>
                    {
                        bs.Entry(existChp).CurrentValues.SetValues(newChp);
                    });
                    bs.SaveChanges();
                }
                OnPropertyChanged(nameof(Book));
            });
        }

        private Book book;
        public Book Book { get => this.book; set => Set(ref this.book, value); }

        public async void Open(Chapter chapter)
        {
            await Navigator.GetForCurrentView().NavigateAsync(typeof(ReadPage), chapter.Book.Id + " " + chapter.Index);
        }

    }
}
