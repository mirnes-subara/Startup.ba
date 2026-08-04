namespace Startupba.Model.SearchObjects
{
    public class StartupImageSearchObject : BaseSearchObject
    {
        public int? StartupId { get; set; }
        public bool? IsCover { get; set; }
        public bool? IsLogo { get; set; }
        public bool? IsActive { get; set; }
    }
}
