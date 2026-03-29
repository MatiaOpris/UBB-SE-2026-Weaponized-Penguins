using System;
using System.ComponentModel;

namespace Boards_WP.Data.Models;

public class Tag
{
    public int TagID { get; init; }
    public Category CategoryBelongingTo { get; set; }
    public string TagName { get; set; } = string.Empty; 
    public string ColorHex {  get; set; } = string.Empty;
}