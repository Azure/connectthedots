namespace Microsoft.ConnectTheDots.Gateway
{
    using System.Diagnostics;

    //--//

    [DebuggerDisplay("OperationCode = {OperationCode}")]
    public sealed class OperationStatus
    {
        internal OperationStatus() { }

        public ErrorCode OperationCode { get; set; }

        public string ErrorMessage { get; set; }

        public bool IsSuccess
        {
            get { return OperationCode == ErrorCode.Success; }
        }
    }

    [DebuggerDisplay("OperationCode = {OperationCode}, Result = {Result}")]
    public sealed class OperationStatus<T>
    {
        internal OperationStatus() { }

        public T Result { get; set; }

        public ErrorCode OperationCode { get; set; }

        public string ErrorMessage { get; set; }

        public bool IsSuccess
        {
            get { return OperationCode == ErrorCode.Success; }
        }
    }
}