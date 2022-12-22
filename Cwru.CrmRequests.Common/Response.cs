using System;

namespace Cwru.CrmRequests.Common
{
    public class Response<T>
    {
        public bool IsSuccessful { get; set; }
        public string ErrorMessage { get; set; }
        public Exception Exception { get; set; }
        public T Payload { get; set; }
        public string ConnectionInfo { get; set; }

        public void EnsureSuccess()
        {
            if (IsSuccessful == false)
            {
                if (Exception != null)
                {
                    throw Exception;
                }
                else
                {
                    throw new Exception("Request failed");
                }
            }
        }
    }
}