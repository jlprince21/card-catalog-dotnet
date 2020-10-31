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
        public Guid ItemId { get; set; }
        public Guid ContainerId { get; set; }
        public string Description { get; set; }
    }
}