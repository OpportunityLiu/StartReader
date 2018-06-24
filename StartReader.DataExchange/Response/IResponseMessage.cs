using StartReader.DataExchange.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartReader.DataExchange.Response
{
    public interface IResponseMessage
    {
    }

    public interface IResponseMessage<TRequest, TResponse> : IResponseMessage
        where TRequest : IRequestMessage<TRequest, TResponse>
        where TResponse : IResponseMessage<TRequest, TResponse>
    {
    }
}
