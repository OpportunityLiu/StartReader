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
        [JsonRequired]
        string ProviderId { get; }
    }

    public interface IRequestMessage<TRequest, TResponse> : IRequestMessage
        where TRequest : RequestMessageBase, IRequestMessage<TRequest, TResponse>
        where TResponse : ResponseMessageBase, IResponseMessage<TRequest, TResponse>
    {
    }
}
