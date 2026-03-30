using System;
using System.ComponentModel;

namespace Boards_WP.Data.Models;

public class Tag
{
    public int TagID { get; init; }
    public required Category CategoryBelongingTo { get; init; }
    public String TagName { get; set; } = String.Empty; 
    public String getColorHex => CategoryBelongingTo.ColorHex;
}