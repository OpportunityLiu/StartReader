using Newtonsoft.Json;
using Opportunity.MvvmUniverse.Collections;
using Opportunity.MvvmUniverse.Services.Navigation;
using Opportunity.MvvmUniverse.Views;
using StartReader.App.Extensiton;
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

        internal async void Search(string queryText)
        {
            var req = new SearchRequest { Keyword = queryText.Trim() };
            SearchResult.Clear();
            foreach (var item in DataSourceManager.Instance.ProviderSources)
            {
                if (!item.IsAvailable)
                    continue;
                foreach (var provider in item.Providers)
                {
                    if (!provider.IsAvailable)
                        continue;
                    try
                    {
                        foreach (var book in (await provider.ExecuteAsync(req).ConfigureAwait(false)).Books)
                        {
                            SearchResult.Add(book);
                        }
                    }
                    catch { }
                }
            }
        }

        public async void Open(BookDataBrief book)
        {
            await Navigator.GetForCurrentView().NavigateAsync(typeof(BookPage), JsonConvert.SerializeObject(book));
        }

        public ObservableList<BookDataBrief> SearchResult { get; } = new ObservableList<BookDataBrief>();
    }
}
