using System;

namespace CardCatalog.Core.ApiModels
{
    public class ApiEditItem
    {
        public string ContainerId { get; set; }
        public string Description { get; set; }
        public string ItemId { get; set; }
    }

    public class SingleItem
    {
        public Guid itemId { get; set; }
        public Guid containerId { get; set; }
        public string itemDescription { get; set; }
        public string containerDescription { get; set; }
    }

    public class ApiItemId
    {
        public Guid itemId { get; set; }
    }
}