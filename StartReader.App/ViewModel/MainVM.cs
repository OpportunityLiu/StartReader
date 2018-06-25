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
            var b = JsonConvert.DeserializeObject<Book>(JsonConvert.SerializeObject(book));
            var dc = SearchResult[book];
            b.CurrentSource = new BookSource
            {
                BookKey = book.Key,
                ExtensionId = dc.ExtensionId,
                PackageFamilyName = dc.PackageFamilyName,
            };
            await Navigator.GetForCurrentView().NavigateAsync(typeof(BookPage), JsonConvert.SerializeObject(b));
        }

        public ObservableDictionary<BookDataBrief, DataSource> SearchResult { get; } = new ObservableDictionary<BookDataBrief, DataSource>();
    }
}
