using System;

namespace Cwru.Publisher.Model
{
    public enum ResultType
    {
        Success = 1,
        Failure = 2,
        Canceled = 3,
    }

    public class Result
    {
        public Result()
        {

        }
        public Result(ResultType resultType, int total, int processed, int failed, Exception exception = null, string errorMessage = null)
        {
            ResultType = resultType;
            Total = total;
            Processed = processed;
            Failed = failed;
            Exception = exception;
            ErrorMessage = errorMessage;
        }
        public ResultType ResultType { get; set; }
        public string ErrorMessage { get; set; }
        public Exception Exception { get; set; }
        public int Total { get; set; }
        public int Processed { get; set; }
        public int Failed { get; set; }
        public string GetErrorMessage()
        {
            return (ErrorMessage ?? Exception?.ToString());
        }
    }
}
