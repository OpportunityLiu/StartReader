using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartReader.DataExchange.Request
{
    public abstract class RequestMessageBase
    {
        [JsonRequired]
        public string ProviderId { get; }
    }
}
