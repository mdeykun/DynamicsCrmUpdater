namespace Cwru.CrmRequests.Common
{
    public class Response<T>
    {
        public bool IsSuccessful { get; set; }
        public string Error { get; set; }
        public T Payload { get; set; }
    }
}