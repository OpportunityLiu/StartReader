using StartReader.App.Config;
using System;

namespace StartReader.App.Extensiton
{
    /// <summary>
    /// 由扩展错误引发的异常
    /// </summary>
    class DataException : Exception
    {
        public DataException(DataSource source, string message)
            : base(message)
        {
            this.DataSource = source ?? throw new ArgumentNullException(nameof(source));
        }
        public DataException(DataSource source, string message, Exception inner)
            : base(message, inner)
        {
            this.DataSource = source ?? throw new ArgumentNullException(nameof(source));
        }

        public DataSource DataSource { get; }
    }

    /// <summary>
    /// 扩展返回的异常，即 <see cref="DataExchange.Response.ErrorResponse"/> 对应的异常
    /// </summary>
    class DataSourceException : DataException
    {
        public DataSourceException(DataSource source, string message, object errorData)
            : base(source, message)
        {
            this.ErrorData = errorData;
        }

        public object ErrorData { get; }

        public override string Message => Settings.Instance.DebugMode
            ? base.Message + "\n" + ErrorData.ToString()
            : base.Message;
    }

    /// <summary>
    /// 扩展返回的数据不符合约束
    /// </summary>
    class DataFormatException : DataException
    {
        public DataFormatException(DataSource source, string debugMessage)
            : base(source, debugMessage)
        {
        }

        public DataFormatException(DataSource source, string debugMessage, Exception inner)
            : base(source, debugMessage, inner)
        {
        }

        public override string Message => Settings.Instance.DebugMode
            ? base.Message
            : $"{DataSource.Extension.DisplayName}（{DataSource.Extension.AppInfo.DisplayInfo.DisplayName}）返回的数据有误。";
    }
}
