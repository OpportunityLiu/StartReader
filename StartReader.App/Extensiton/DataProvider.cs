using Opportunity.MvvmUniverse;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace StartReader.App.Extensiton
{
    class DataProvider : ObservableObject
    {
        public string Id { get; }
        public IPropertySet Properties { get; }

        private bool isAvailable;
        public bool IsAvailable { get => this.isAvailable; private set => Set(ref this.isAvailable, value); }

        private string displayName;
        public string DisplayName { get => this.displayName; private set => Set(ref this.displayName, value); }

        private Uri url;
        public Uri Url { get => this.url; private set => Set(ref this.url, value); }

        private string appServiceName;
        public string AppServiceName { get => this.appServiceName; private set => Set(ref this.appServiceName, value); }

        private string description;
        public string Description { get => this.description; private set => Set(ref this.description, value); }

        public DataProvider(IPropertySet providerData)
        {
            Id = providerData.GetProperty(nameof(Id));
            if (Id.IsNullOrWhiteSpace())
                throw new ArgumentNullException("Id of Provider is not defined or empty");
            this.Properties = providerData;
            LoadData(providerData);
        }

        public void LoadData(IPropertySet providerData)
        {
            Debug.Assert(providerData.GetProperty(nameof(Id)) == Id);
            try
            {
                DisplayName = providerData.GetProperty(nameof(DisplayName)).CoalesceNullOrWhiteSpace(Id);
                AppServiceName = providerData.GetProperty(nameof(AppServiceName));
                if (AppServiceName.IsNullOrWhiteSpace())
                    throw new ArgumentNullException("AppServiceName of Provider is not defined or empty");
                Url = new Uri(providerData.GetProperty(nameof(Url)));
                Description = string.Join(Environment.NewLine, providerData.GetProperties(nameof(Description))).CoalesceNullOrWhiteSpace("");
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
