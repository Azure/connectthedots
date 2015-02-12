using System;

namespace Gateway.Utils.OperationStatus
{
    public static class OperationStatusFactory
    {
        private static readonly OperationStatus _SuccessConst = new OperationStatus { OperationCode = ErrorCode.Success };

        public static OperationStatus CreateSuccess()
        {
            return _SuccessConst;
        }

        public static OperationStatus<T> CreateSuccess<T>(T result)
        {
            return new OperationStatus<T> { OperationCode = ErrorCode.Success, Result = result };
        }

        public static OperationStatus CreateError(ErrorCode errorCode)
        {
            return new OperationStatus { OperationCode = errorCode };
        }


        public static OperationStatus CreateError(ErrorCode errorCode, string errorMessage)
        {
            return new OperationStatus { OperationCode = errorCode, ErrorMessage = errorMessage };
        }

        public static OperationStatus CreateError(ErrorCode errorCode, Exception exception)
        {
            return new OperationStatus { OperationCode = errorCode, ErrorMessage = exception.Message + exception.StackTrace };
        }

        public static OperationStatus<T> CreateError<T>(ErrorCode errorCode)
        {
            return new OperationStatus<T> { OperationCode = errorCode, Result = default(T) };
        }

        public static OperationStatus<T> CreateError<T>(ErrorCode errorCode, Exception exception)
        {
            return new OperationStatus<T> { OperationCode = errorCode, ErrorMessage = exception.Message + exception.StackTrace, Result = default(T) };
        }

        public static OperationStatus<T> CreateError<T>(ErrorCode errorCode, string errorMessage, T result = default (T))
        {
            return new OperationStatus<T> { OperationCode = errorCode, ErrorMessage = errorMessage, Result = result };
        }

        public static OperationStatus<T> CreateError<T>(ErrorCode errorCode, string errorMessage, Exception exception)
        {
            return new OperationStatus<T> { OperationCode = errorCode, ErrorMessage = errorMessage + exception.Message + exception.StackTrace, Result = default(T) };
        }

        public static OperationStatus CopyFrom(OperationStatus source)
        {
            return new OperationStatus { OperationCode = source.OperationCode, ErrorMessage = source.ErrorMessage };
        }

        public static OperationStatus CopyFrom<T>(OperationStatus<T> source)
        {
            return new OperationStatus { OperationCode = source.OperationCode, ErrorMessage = source.ErrorMessage };
        }

        public static OperationStatus<TOut> CopyFrom<TOut>(OperationStatus source)
        {
            return new OperationStatus<TOut> { OperationCode = source.OperationCode, ErrorMessage = source.ErrorMessage, Result = default(TOut) };
        }

        public static OperationStatus<TOut> CopyFrom<TIn, TOut>(OperationStatus<TIn> source, TOut result = default (TOut))
        {
            return new OperationStatus<TOut> { OperationCode = source.OperationCode, ErrorMessage = source.ErrorMessage, Result = result };
        }
    }
}