using System;

namespace Startupba.Model.Responses
{
    public class SupportTicketResponse
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 0=Open, 1=Answered, 2=Closed
        /// </summary>
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;

        public string? AdminResponse { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? AnsweredAt { get; set; }
        public DateTime? ClosedAt { get; set; }
    }
}
