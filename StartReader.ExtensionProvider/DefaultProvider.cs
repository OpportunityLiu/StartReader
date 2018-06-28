using StartReader.DataExchange;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;

namespace StartReader.ExtensionProvider
{
    public sealed class DefaultProvider : IBackgroundTask
    {
        private BackgroundTaskDeferral def;
        private DataProvider provider;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            this.def = taskInstance.GetDeferral();
            taskInstance.Canceled += this.TaskInstance_Canceled;
            var de = (AppServiceTriggerDetails)taskInstance.TriggerDetails;
            this.provider = de.AttachProvider()
                .Add<MiaoBiGe>("miaobige")
                .Add<TwoKXiaoShuo>("2kxs")
                .Provider;
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            (this.provider as IDisposable)?.Dispose();
            this.def.Complete();
        }
    }
}
