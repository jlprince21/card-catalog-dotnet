namespace CardCatalog.Api.Models
{
    public class ApiNewTag
    {
        public string Title { get; set; }
    }

    public class ApiNewContainer
    {
        public string containerDescription { get; set; }
    }

    public class ApiNewItem
    {
        public string containerId { get; set; }
        public string itemDescription { get; set; }
    }

    public class ApiDeleteItem
    {
        public string itemId { get; set; }
    }
}