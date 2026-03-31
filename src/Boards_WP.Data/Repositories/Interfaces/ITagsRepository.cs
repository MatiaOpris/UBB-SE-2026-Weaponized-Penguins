using System;
using System.Collections.Generic;
using System.Text;

using Boards_WP.Data.Models;

namespace Boards_WP.Data.Repositories.Interfaces;

public interface ITagsRepository
{
    public List<Category> GetAllCategories();
    public void AddTag(Tag t);
    public int GetCategoryCount();
}
