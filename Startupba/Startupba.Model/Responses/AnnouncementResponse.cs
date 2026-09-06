using System;

namespace Startupba.Model.Responses
{
    public class AnnouncementResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        public int CreatedByUserId { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public string CreatedByUserName
        {
            get => CreatedByName;
            set => CreatedByName = value;
        }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public byte[]? ImageData { get; set; }
    }
}
