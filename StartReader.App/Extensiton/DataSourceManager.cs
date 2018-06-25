using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppExtensions;

namespace StartReader.App.Extensiton
{
    class DataSourceManager : ObservableObject
    {
        public static DataSourceManager Instance { get; } = new DataSourceManager();

        private readonly AppExtensionCatalog catalog;

        private readonly ObservableList<DataSource> sources = new ObservableList<DataSource>();
        public ObservableListView<DataSource> Sources => this.sources.AsReadOnly();

        private readonly Timer timer;

        private void timerCallback(object state)
        {
            try
            {
                foreach (var item in this.sources)
                {
                    if (item.IsOpened && DateTime.UtcNow - item.LastUse > TimeSpan.FromMinutes(3))
                        item.Close();
                }
            }
            catch (InvalidOperationException) { }
        }

        private DataSourceManager()
        {
            this.catalog = AppExtensionCatalog.Open("StartReader.DataProviderSource");
            this.catalog.PackageStatusChanged += this.refresh;
            this.catalog.PackageInstalled += this.refresh;
            this.catalog.PackageUpdated += this.refresh;
            this.catalog.PackageUninstalling += this.Catalog_PackageUninstalling;
            var ignore = RefreshAsync();
            this.timer = new Timer(timerCallback, null, 60_000, 60_000);
        }

        private void Catalog_PackageUninstalling(AppExtensionCatalog sender, AppExtensionPackageUninstallingEventArgs args)
        {
            var pfn = args.Package.Id.FamilyName;
            var ext = this.sources.Where(ds => ds.PackageFamilyName == pfn).ToList();
            foreach (var item in ext)
            {
                item.Close();
                this.sources.Remove(item);
            }
        }

        private async void refresh(AppExtensionCatalog sender, object args)
        {
            await RefreshAsync();
        }

        public async Task RefreshAsync()
        {
            var ext = await this.catalog.FindAllAsync();
            this.sources.Update(ext.Select(e => new DataSource(e)).ToList(),
                (s1, s2) => s1.PackageFamilyName.CompareTo(s2.PackageFamilyName),
                (oldS, newS) => { var ignore = oldS.LoadDataAsync(newS.Extension); });
        }

        public async Task<bool> RequestRemovePackageAsync(DataSource dataSource)
        {
            if (dataSource.IsInternal)
                return false;
            return await this.catalog.RequestRemovePackageAsync(dataSource.Extension.Package.Id.FullName);
        }
    }
}
