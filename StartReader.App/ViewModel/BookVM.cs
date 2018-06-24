using Newtonsoft.Json;
using Opportunity.MvvmUniverse.Views;
using StartReader.App.Model;

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
        }

        public Book Book { get; }
    }
}
