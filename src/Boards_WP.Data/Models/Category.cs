using System;
using System.Collections.Generic;
using System.Text;

namespace Boards_WP.Data.Models;

public class Category
{
    public int CategoryID { get; init; }
    
    public String CategoryName { get; set; } = String.Empty;

    public String ColorHex { get; set; } = String.Empty;
}
