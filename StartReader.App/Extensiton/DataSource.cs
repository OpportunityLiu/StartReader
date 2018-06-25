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

namespace StartReader.App.Extensiton
{
    [DebuggerDisplay(@"Name = {Extension.DisplayName} PFN = {PackageFamilyName}")]
    internal sealed partial class DataSource : ObservableObject
    {
        public DataSource(AppExtension extension)
        {
            this.PackageFamilyName = extension.AppInfo.PackageFamilyName;
            this.ExtensionId = extension.Id;
            var ignore = LoadDataAsync(extension);
        }

        public string PackageFamilyName { get; }
        public bool IsInternal => PackageFamilyName == Package.Current.Id.FamilyName;

        public string ExtensionId { get; }

        private AppExtension extension;
        public AppExtension Extension { get => this.extension; private set => Set(ref this.extension, value); }

        private bool isAvailable;
        public bool IsAvailable { get => this.isAvailable; private set => Set(ref this.isAvailable, value); }

        private Uri url;
        public Uri Url { get => this.url; private set => Set(ref this.url, value); }

        private string appServiceName;
        public string AppServiceName { get => this.appServiceName; private set => Set(ref this.appServiceName, value); }

        public async Task LoadDataAsync(AppExtension extension)
        {
            Debug.Assert(extension.AppInfo.PackageFamilyName == PackageFamilyName && extension.Id == ExtensionId);
            this.Extension = extension;
            try
            {
                var prop = await this.extension.GetExtensionPropertiesAsync();
                Url = new Uri(prop.GetProperty("Url"));
                AppServiceName = prop.GetProperty("AppService");
            }
            catch
            {
                IsAvailable = false;
                return;
            }
            IsAvailable = true;
        }

    }
}
