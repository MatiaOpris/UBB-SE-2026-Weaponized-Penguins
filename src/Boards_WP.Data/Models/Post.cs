using System;

namespace Boards_WP.Data.Models;

public class Post
{
    public int PostID { get; init; }
    public User Owner { get; set; }
    public Community Community { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public byte[]? Image { get; set; }
    public int Score { get; set; }
    public int CommentsNumber { get; set; }
    public DateTime CreationTime { get; set; }
    public List<Tag> Tags { get; set; } = new List<Tag>();
}