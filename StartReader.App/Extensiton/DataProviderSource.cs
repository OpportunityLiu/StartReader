using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppExtensions;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace StartReader.App.Extensiton
{
    class DataProviderSource : ObservableObject
    {
        public DataProviderSource(AppExtension extension)
        {
            this.PackageFamilyName = extension.AppInfo.PackageFamilyName;
            var ignore = LoadDataAsync(extension);
        }

        public string PackageFamilyName { get; }

        private AppExtension extension;
        public AppExtension Extension { get => this.extension; private set => Set(ref this.extension, value); }

        private bool isAvailable;
        public bool IsAvailable { get => this.isAvailable; set => Set(ref this.isAvailable, value); }

        public async Task LoadDataAsync(AppExtension extension)
        {
            Debug.Assert(extension.AppInfo.PackageFamilyName == PackageFamilyName);
            this.Extension = extension;
            try
            {
                var prop = await this.extension.GetExtensionPropertiesAsync();
                var providers = prop.GetChild("Providers").GetChildren("Provider");
                this.providers.Update(providers.Select(e =>
                {
                    try
                    {
                        return new DataProvider(this, e);
                    }
                    catch
                    {
                        return null;
                    }
                }).Where(e => e != null).ToList(),
                    (s1, s2) => s1.Id.CompareTo(s2.Id),
                    (oldP, newP) => oldP.LoadData(newP.Properties));
            }
            catch
            {
                IsAvailable = false;
                return;
            }
            IsAvailable = true;
        }

        private readonly ObservableList<DataProvider> providers = new ObservableList<DataProvider>();
        public ObservableListView<DataProvider> Providers => this.providers.AsReadOnly();
    }
}
