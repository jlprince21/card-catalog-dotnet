using Newtonsoft.Json;
using System.Collections.Generic;

namespace CardCatalog.Api.Models
{
    public class PagedResponse<T> : Response<T>
    {
        private int totalRecords;

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get => System.Convert.ToInt32(System.Math.Ceiling((double) totalRecords) / ((double) PageSize)); }
        public int TotalRecords { get => totalRecords; set => totalRecords = value; }
        public List<T> Items { get; set; }

        public PagedResponse(List<T> items) : base()
        {
            this.Errors = null;
            this.Items = items;
            this.Message = null;
            this.Success = true;
        }
    }
}