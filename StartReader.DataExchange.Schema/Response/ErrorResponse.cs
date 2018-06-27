using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartReader.DataExchange.Response
{
    public sealed class ErrorResponse : ResponseMessageBase, IResponseMessage
    {
        public static ErrorResponse NotImplemented { get; } = new ErrorResponse(1, "该方法未实现。", null);

        public int Code { get; }
        public string Message { get; }
        public object Data { get; }

        public ErrorResponse(int code, string message, object data)
        {
            if (code == 0)
                throw new ArgumentException("code 不能为 0", nameof(code));
            this.Code = code;
            this.Message = message ?? "";
            this.Data = data;
        }
    }
}
