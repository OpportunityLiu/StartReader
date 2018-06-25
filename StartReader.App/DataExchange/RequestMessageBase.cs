using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace StartReader.DataExchange.Request
{
    public abstract class RequestMessageBase
    {
        [JsonRequired]
        public string ProviderId { get; internal set; }
    }
}
