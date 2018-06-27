using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Storage;
using Windows.Storage;

namespace StartReader.App.Config
{
    class Settings : StorageObject
    {
        private Settings() : base("Settings")
        {
        }

        public static Settings Instance { get; } = new Settings()
        {
#if DEBUG
            DebugMode = true,
#endif
        };

        [ApplicationSetting(ApplicationDataLocality.Local)]
        public bool DebugMode { get => GetStorage<bool>(); set => SetStorage(value); }
    }
}
