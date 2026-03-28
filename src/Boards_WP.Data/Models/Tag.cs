using System;
using System.ComponentModel;

namespace Boards_WP.Data.Models;

public class Tag
{
    public int TagID { get; set; }
    public Category CategoryBelongingTo { get; set; }
    public String TagName { get; set; } = String.Empty; 
}