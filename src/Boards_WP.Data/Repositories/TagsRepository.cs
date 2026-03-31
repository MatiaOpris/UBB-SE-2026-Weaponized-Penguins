using System.Collections.Generic;

using Microsoft.Data.SqlClient;

using Boards_WP.Data.Models;
using Boards_WP.Data.Repositories.Interfaces;

namespace Boards_WP.Data.Repositories;

public class TagsRepository : ITagsRepository
{
    private readonly string _connectionString;

    public TagsRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<Category> GetAllCategories()
    {
        var categories = new List<Category>();
        using var connection = new SqlConnection(_connectionString);


        const string query = "SELECT categoryID, categoryName, categoryColor FROM Categories";
        using var command = new SqlCommand(query, connection);

        connection.Open();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            categories.Add(new Category
            {

                CategoryID = reader.GetInt32(reader.GetOrdinal("categoryID")),
                CategoryName = reader.GetString(reader.GetOrdinal("categoryName")),
                ColorHex = reader.GetString(reader.GetOrdinal("categoryColor"))
            });
        }

        return categories;
    }

    public void AddTag(Tag t)
    {
        using var connection = new SqlConnection(_connectionString);

        const string query = "INSERT INTO Tags (tagCategoryID, tagName) VALUES (@CategoryID, @TagName)";
        using var command = new SqlCommand(query, connection);

        command.Parameters.AddWithValue("@CategoryID", t.CategoryBelongingTo.CategoryID);
        command.Parameters.AddWithValue("@TagName", t.TagName);

        connection.Open();
        command.ExecuteNonQuery();
    }

    public int GetCategoryCount()
    {
        using var connection = new SqlConnection(_connectionString);
        const string query = "SELECT COUNT(*) FROM Categories";
        using var command = new SqlCommand(query, connection);

        connection.Open();
        return (int)command.ExecuteScalar();
    }
}