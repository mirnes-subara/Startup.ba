namespace Startupba.Model.SearchObjects
{
    public class SupportTicketSearchObject : BaseSearchObject
    {
        public int? UserId { get; set; }

        /// <summary>
        /// 0=Open, 1=Answered, 2=Closed
        /// </summary>
        public int? Status { get; set; }
    }
}
