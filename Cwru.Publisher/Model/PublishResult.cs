using System;

namespace Cwru.Publisher.Model
{
    public class Result
    {
        public Result(int total, int processed, int failed, Exception exception = null)
        {
            Total = total;
            Processed = processed;
            Failed = failed;
            Exception = exception;
        }

        public Exception Exception { get; set; }
        public int Total { get; set; }
        public int Processed { get; set; }
        public int Failed { get; set; }
    }
}
