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
            ViewModelFactory.Register(s => new BookVM(JsonConvert.DeserializeObject<Book>(s)));
        }

        private BookVM(Book book)
        {
            this.Book = book;
            Task.Run(async () =>
            {
                this.Detailed = (await Book.CurrentSource.FindSource().ExecuteAsync(new GetBookRequest
                {
                    BookKey = book.CurrentSource.BookKey,
                })).BookData;
            });
        }

        public Book Book { get; }

        private BookDataDetailed detailed;
        public BookDataDetailed Detailed { get => this.detailed; set => Set(ref this.detailed, value); }

        public async void Open(ChapterDataBrief chapter)
        {
            var c = JsonConvert.DeserializeObject<Chapter>(JsonConvert.SerializeObject(chapter));
            c.Book = Book;
            c.Source = Book.CurrentSource;
            await Navigator.GetForCurrentView().NavigateAsync(typeof(ReadPage), JsonConvert.SerializeObject(c));
        }

    }
}
