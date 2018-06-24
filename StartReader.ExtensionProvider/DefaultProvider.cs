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
        private DataExchangeProvider provider;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            this.def = taskInstance.GetDeferral();
            taskInstance.Canceled += this.TaskInstance_Canceled;
            var de = (AppServiceTriggerDetails)taskInstance.TriggerDetails;
            switch (de.Name)
            {
            case "miaobige":
                this.provider = new MiaoBiGeProvider(de.AppServiceConnection);
                break;
            }
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            this.def.Complete();
        }
    }
}
