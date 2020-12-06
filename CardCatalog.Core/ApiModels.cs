using System;

namespace CardCatalog.Core.ApiModels
{
    public class ApiEditItem
    {
        public string containerId { get; set; }
        public string description { get; set; }
        public string itemId { get; set; }
    }

     public class ApiEditContainer
    {
        public string containerId { get; set; }
        public string description { get; set; }
    }

    public class ApiMoveItem
    {
        public string itemId { get; set; }
        public string containerId { get; set; }
    }

    public class SingleItem
    {
        public Guid itemId { get; set; }
        public Guid containerId { get; set; }
        public string itemDescription { get; set; }
        public string containerDescription { get; set; }
    }

    public class SingleContainer
    {
        public Guid id { get; set; }
        public string description { get; set; }
    }

    public class ApiItemId
    {
        public Guid itemId { get; set; }
    }
}