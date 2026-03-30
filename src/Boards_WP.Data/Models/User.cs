using System;

namespace Boards_WP.Data.Models
{
    public class User
    {
        public int UserID { get; init; }
        public String Username { get; set; } = String.Empty;
        public String Email { get; set; } = String.Empty;
        public String PasswordHash { get; set; } = String.Empty;
        public string AvatarUrl { get; set; } = String.Empty;
        public string Bio { get; set; } = String.Empty;
        public string Status { get; set; } = String.Empty;
    }
}