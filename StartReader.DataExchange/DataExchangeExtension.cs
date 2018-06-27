using Newtonsoft.Json;
using StartReader.DataExchange.Request;
using StartReader.DataExchange.Response;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace StartReader.DataExchange
{
    public static class DataExchangeExtension
    {
        private static ValueSet ToValueSet(this IResponseMessage message)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));
            if (message is ErrorResponse er)
                return new ValueSet
                {
                    ["Code"] = er.Code,
                    ["Message"] = er.Message,
                    ["Data"] = JsonConvert.SerializeObject(er.Data),
                };
            return new ValueSet
            {
                ["Code"] = 0,
                ["Data"] = JsonConvert.SerializeObject(message),
            };
        }

        public static IAsyncOperation<AppServiceResponseStatus> SendResponseAsync(this AppServiceRequest request, IResponseMessage message)
        {
            var rqmsg = request.Message;
            var method = rqmsg["Method"].ToString();
            var rsType = DataExchangeMap.RequestToResponseMap[DataExchangeMap.MethodToRequestMap[method]];
            if (!(message is ErrorResponse || message.GetType() == rsType))
                throw new ArgumentException($"类型错误，应为 {rsType} 或 {typeof(ErrorResponse)}。", nameof(message));
            return request.SendResponseAsync(message.ToValueSet());
        }

        public static IRequestMessage GetMessage(this AppServiceRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));
            var message = request.Message;
            var method = message["Method"].ToString();
            var data = message["Data"].ToString();
            var requestType = DataExchangeMap.MethodToRequestMap[method];
            return (IRequestMessage)JsonConvert.DeserializeObject(data, requestType);
        }

    }
}
