using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppExtensions;

namespace StartReader.App.Extensiton
{
    class DataSourceManager : ObservableObject
    {
        public static DataSourceManager Instance { get; } = new DataSourceManager();

        private readonly AppExtensionCatalog catalog;

        private readonly ObservableList<DataProviderSource> providerSources = new ObservableList<DataProviderSource>();
        public ObservableListView<DataProviderSource> ProviderSources => this.providerSources.AsReadOnly();

        private DataSourceManager()
        {
            this.catalog = AppExtensionCatalog.Open("StartReader.DataProviderSource");
            this.catalog.PackageStatusChanged += this.Catalog_PackageStatusChanged;
        }

        private async void Catalog_PackageStatusChanged(AppExtensionCatalog sender, AppExtensionPackageStatusChangedEventArgs args)
        {
            if (ProviderSources != null)
                await RefreshAsync();
        }

        public async Task RefreshAsync()
        {
            var ext = await this.catalog.FindAllAsync();
            this.providerSources.Update(ext.Select(e => new DataProviderSource(e)).ToList(),
                (s1, s2) => s1.PackageFamilyName.CompareTo(s2.PackageFamilyName),
                (oldS, newS) => { var ignore = oldS.LoadDataAsync(newS.Extension); });
        }
    }
}
