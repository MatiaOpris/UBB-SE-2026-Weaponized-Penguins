using System;

namespace Boards_WP.Data.Models
{
    public class Notification
    {
        public int NotificationID { get; set; }
        public DateTime CreationTime { get; init; } = DateTime.UtcNow;
        public required Post RelatedPost { get; init; }
        public required User Receiver { get; init; }
        public required User Actor { get; init; }
        public NotificationType ActionType { get; init; }
        public bool IsRead { get; set; } = false;
    }
}
