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

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            this.def = taskInstance.GetDeferral();
            taskInstance.Canceled += this.TaskInstance_Canceled;
            var de = (AppServiceTriggerDetails)taskInstance.TriggerDetails;
            var p = new MiaoBiGeProvider(de.AppServiceConnection);
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            this.def.Complete();
        }
    }

    internal sealed class MiaoBiGeProvider : DataExchangeProvider
    {
        public MiaoBiGeProvider(AppServiceConnection connection) : base(connection)
        {
        }
    }
}
