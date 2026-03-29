using System;
using System.Collections.Generic;
using System.Text;

namespace Boards_WP.Data.Models;

public class Category
{
    public int CategoryID { get; init; }
    
    public string CategoryName { get; set; }

    public string ColorHex { get; set; }
}
