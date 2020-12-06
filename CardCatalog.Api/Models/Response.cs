using Newtonsoft.Json;

namespace CardCatalog.Api.Models
{
    public class Response<T>
    {
        public string[] Errors { get; set;}
        public string Message { get; set;}
        public bool Success { get; set;}

        public Response()
        {
            Errors = null;
            Message = string.Empty;
            Success = true;
        }
    }
}