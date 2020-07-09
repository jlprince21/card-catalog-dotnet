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
        public string Id { get; set; }
    }
}