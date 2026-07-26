namespace Startupba.Model.SearchObjects
{
    public class CommentSearchObject : BaseSearchObject
    {
        public int? BlogPostId { get; set; }
        public int? UserId { get; set; }
        public bool? IsActive { get; set; }
    }
}
