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
        where TRequest : RequestMessageBase, IRequestMessage<TRequest, TResponse>
        where TResponse : ResponseMessageBase, IResponseMessage<TRequest, TResponse>
    {
    }
}
