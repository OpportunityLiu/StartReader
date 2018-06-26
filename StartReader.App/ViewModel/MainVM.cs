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
                var existBook = await bs.Books.Include(b => b.Sources).FirstOrDefaultAsync(b => b.Title == book.Title && b.Author == book.Author);
                var dc = SearchResult[book];
                var newBook = JsonConvert.DeserializeObject<Book>(JsonConvert.SerializeObject(book));
                var newSource = new BookSource
                {
                    BookId = existBook?.Id ?? 0,
                    IsCurrent = true,
                    BookKey = book.Key,
                    ExtensionId = dc.ExtensionId,
                    PackageFamilyName = dc.PackageFamilyName,
                };
                if (existBook is null)
                {
                    newBook.Sources.Add(newSource);
                    bs.Books.Add(newBook);
                }
                else
                {
                    newBook.Id = existBook.Id;
                    bs.Entry(existBook).CurrentValues.SetValues(newBook);
                    var existSource = existBook.Sources.FirstOrDefault(s
                        => s.ExtensionId == newSource.ExtensionId
                        && s.PackageFamilyName == newSource.PackageFamilyName);
                    if (existSource is null)
                        existBook.Sources.Add(newSource);
                    else
                    {
                        var oldCurrentSource = existBook.Sources.First(s => s.IsCurrent);
                        oldCurrentSource.IsCurrent = false;
                        newSource.Id = existSource.Id;
                        bs.Entry(existSource).CurrentValues.SetValues(newSource);
                    }
                }
                await bs.SaveChangesAsync();
                await Navigator.GetForCurrentView().NavigateAsync(typeof(BookPage), (existBook ?? newBook).Id.ToString());
            }
        }

        public ObservableDictionary<BookDataBrief, DataSource> SearchResult { get; } = new ObservableDictionary<BookDataBrief, DataSource>();
    }
}
