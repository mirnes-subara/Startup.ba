namespace Startupba.Model.SearchObjects
{
    public class BlogPostSearchObject : BaseSearchObject
    {
        public string? Title { get; set; }
        public int? AuthorId { get; set; }
        public int? StartupId { get; set; }
        public bool? IsActive { get; set; }
    }
}
