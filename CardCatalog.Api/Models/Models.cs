namespace CardCatalog.Api.Models
{
    public class ApiNewTag
    {
        public string Title { get; set; }
    }

    public class ApiNewContainer
    {
        public string Description { get; set; }
    }

    public class ApiNewItem
    {
        public string ContainerId { get; set; }
        public string Description { get; set; }
    }

    public class ApiDeleteItem
    {
        public string ItemId { get; set; }
    }

    public class ApiResponseBase
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}