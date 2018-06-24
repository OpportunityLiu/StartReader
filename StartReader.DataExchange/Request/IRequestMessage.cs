using Newtonsoft.Json;
using StartReader.DataExchange.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartReader.DataExchange.Request
{
    public interface IRequestMessage
    {
        [JsonIgnore]
        string Method { get; }
    }

    public abstract class RequestMessageBase
    {
        [JsonRequired]
        public string ProviderId { get; internal set; }
    }

    public interface IRequestMessage<TRequest, TResponse> : IRequestMessage
        where TRequest : IRequestMessage<TRequest, TResponse>
        where TResponse : IResponseMessage<TRequest, TResponse>
    {
    }
}
