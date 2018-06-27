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
    class MainVM : ViewModelBase
    {
        static MainVM()
        {
            ViewModelFactory.Register(s => new MainVM());
        }

        private MainVM()
        {
        }

        internal async void Search(string queryText)
        {
            var req = new SearchRequest { Keyword = queryText.Trim() };
            SearchResult.Clear();
            foreach (var item in DataSourceManager.Instance.Sources)
            {
                if (!item.IsAvailable)
                    continue;
                try
                {
                    foreach (var book in (await item.ExecuteAsync(req).ConfigureAwait(false)).Books)
                    {
                        SearchResult.Add(book, item);
                    }
                }
                catch { }
            }
        }

        internal DataSourceManager DataSourceManager => DataSourceManager.Instance;

        public async void Open(BookDataBrief book)
        {
            using (var bs = BookShelf.Create())
            {
                var dc = SearchResult[book];
                var existBook = await bs.Books.FirstOrDefaultAsync(b
                    => b.Title == book.Title
                    && b.Author == book.Author
                    && b.PackageFamilyName == dc.PackageFamilyName
                    && b.ExtensionId == dc.ExtensionId);
                if (existBook is null)
                {
                    existBook = new Book(book, dc);
                    bs.Books.Add(existBook);
                }
                else
                {
                    existBook.Update(book, dc);
                }
                await bs.SaveChangesAsync();
                await Navigator.GetForCurrentView().NavigateAsync(typeof(BookPage), existBook.Id.ToString());
            }
        }

        public ObservableDictionary<BookDataBrief, DataSource> SearchResult { get; } = new ObservableDictionary<BookDataBrief, DataSource>();
    }
}
