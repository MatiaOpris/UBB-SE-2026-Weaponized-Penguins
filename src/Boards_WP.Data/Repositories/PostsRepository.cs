using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;

using Boards_WP.Data.Models;
using Boards_WP.Data.Repositories.Interfaces;

using Microsoft.Data.SqlClient;

namespace Boards_WP.Data.Repositories;

public class PostsRepository : IPostsRepository
{
    private readonly string _connectionString;

    public PostsRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void AddPost(Post p)
    {
        using var connection = new SqlConnection(_connectionString);


        const string insertPostQuery = @"
        INSERT INTO Posts (ownerID, communityID, title, description, image, score, commentsNumber) 
        VALUES (@OwnerID, @CommunityID, @Title, @Description, @Image, @Score, @CommentsNumber);
        
        SELECT CAST(SCOPE_IDENTITY() AS INT);"; 

        using var command = new SqlCommand(insertPostQuery, connection);
        command.Parameters.AddWithValue("@OwnerID", p.Owner.UserID);
        command.Parameters.AddWithValue("@CommunityID", p.ParentCommunity.CommunityID);
        command.Parameters.AddWithValue("@Title", p.Title);
        command.Parameters.AddWithValue("@Description", p.Description);

        var imageParameter = new SqlParameter("@Image", SqlDbType.VarBinary);
        imageParameter.Value = (object?)p.Image ?? DBNull.Value;
        command.Parameters.Add(imageParameter);

        command.Parameters.AddWithValue("@Score", p.Score);
        command.Parameters.AddWithValue("@CommentsNumber", p.CommentsNumber);

        connection.Open();

        int newPostId = (int)command.ExecuteScalar();

        
        if (p.Tags != null && p.Tags.Count > 0)
        {
            const string insertTagQuery = @"
            INSERT INTO PostTags (postID, tagID, position) 
            VALUES (@PostID, @TagID, @Position)";

            for (int i = 0; i < p.Tags.Count; i++)
            {
                if (i > 9) break; 

                using var tagCommand = new SqlCommand(insertTagQuery, connection);
                tagCommand.Parameters.AddWithValue("@PostID", newPostId);
                tagCommand.Parameters.AddWithValue("@TagID", p.Tags[i].TagID);
                tagCommand.Parameters.AddWithValue("@Position", i);
                tagCommand.ExecuteNonQuery();
            }
        }
    }


    //works with postview - useful for service like/disklike logic
    public VoteType GetUserVoteForPost(int userId, int postId)
    {
        using var connection = new SqlConnection(_connectionString);
        const string query = "SELECT vote FROM PostsViews WHERE userID = @UserID AND postID = @PostID";
        using var command = new SqlCommand(query, connection);

        command.Parameters.AddWithValue("@UserID", userId);
        command.Parameters.AddWithValue("@PostID", postId);

        connection.Open();
        var result = command.ExecuteScalar();


        if (result == null) return VoteType.None;

        return (VoteType)Convert.ToInt32(result);
    }

    public void SetUserVoteForPost(int userId, int postId, VoteType vote)
    {
        using var connection = new SqlConnection(_connectionString);

        const string query = @"
        IF @Vote = 0
            DELETE FROM PostsViews WHERE userID = @UserID AND postID = @PostID;
        ELSE
        BEGIN
            UPDATE PostsViews SET vote = @Vote WHERE userID = @UserID AND postID = @PostID;
            IF @@ROWCOUNT = 0
                INSERT INTO PostsViews (userID, postID, vote) VALUES (@UserID, @PostID, @Vote);
        END";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UserID", userId);
        command.Parameters.AddWithValue("@PostID", postId);

        command.Parameters.AddWithValue("@Vote", (int)vote);

        connection.Open();
        command.ExecuteNonQuery();
    }

    public void DeletePost(int postID)
    {
        ExecuteSimpleUpdate("DELETE FROM Posts WHERE postID = @ID", postID);
    }

    public void IncreaseScore(int postID)
    {
        ExecuteSimpleUpdate("UPDATE Posts SET score = score + 1 WHERE postID = @ID", postID);
    }

    public void DecreaseScore(int postID)
    {
        ExecuteSimpleUpdate("UPDATE Posts SET score = score - 1 WHERE postID = @ID", postID);
    }

    public void IncreaseCommentsNumber(int postID)
    {
        ExecuteSimpleUpdate("UPDATE Posts SET commentsNumber = commentsNumber + 1 WHERE postID = @ID", postID);
    }

    
    private void ExecuteSimpleUpdate(string query, int id)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);

        command.Parameters.AddWithValue("@ID", id);

        connection.Open();
        command.ExecuteNonQuery();
    }

    public Post GetPostByPostID(int postID)
    {
        using var connection = new SqlConnection(_connectionString);


        const string query = @"
        SELECT p.*, 
               u.username AS owner_username, u.email AS owner_email, u.avatarUrl AS owner_avatarUrl, u.bio AS owner_bio, u.status AS owner_status,
               c.name AS community_name, c.description AS community_description,
               adm.userID AS admin_userID, adm.username AS admin_username
        FROM Posts p
        JOIN Users u ON p.ownerID = u.userID
        JOIN Communities c ON p.communityID = c.communityID
        JOIN Users adm ON c.adminID = adm.userID
        WHERE p.postID = @ID;

        SELECT t.tagID, t.tagName, cat.categoryID, cat.categoryName, cat.categoryColor, pt.position
        FROM Tags t
        JOIN PostTags pt ON t.tagID = pt.tagID
        JOIN Categories cat ON t.tagCategoryID = cat.categoryID
        WHERE pt.postID = @ID
        ORDER BY pt.position;"; 

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ID", postID);

        connection.Open();
        using var reader = command.ExecuteReader();

        Post post = null!;

        if (reader.Read())
        {
            post = MapReaderToPost(reader); 
            post.Tags = new List<Tag>();  
        }

        if (post != null && reader.NextResult())
        {
            while (reader.Read())
            {
                var category = new Category
                {
                    CategoryID = reader.GetInt32(reader.GetOrdinal("categoryID")),
                    CategoryName = reader.GetString(reader.GetOrdinal("categoryName")),
                    ColorHex = reader.GetString(reader.GetOrdinal("categoryColor"))
                };

                post.Tags.Add(new Tag
                {
                    TagID = reader.GetInt32(reader.GetOrdinal("tagID")),
                    TagName = reader.GetString(reader.GetOrdinal("tagName")),
                    CategoryBelongingTo = category
                });
            }
        }

        return post;
    }

    public List<Post> GetPostsByCommunityIDs(int[] communityIDs)
    {
        if (communityIDs.Length == 0) return new List<Post>();

        string idList = string.Join(",", communityIDs);
        string query = $@"
            SELECT p.*, 
                   u.username AS owner_username, u.email AS owner_email, u.avatarUrl AS owner_avatarUrl, u.bio AS owner_bio, u.status AS owner_status,
                   c.name AS community_name, c.description AS community_description,
                   adm.userID AS admin_userID, adm.username AS admin_username
            FROM Posts p
            JOIN Users u ON p.ownerID = u.userID
            JOIN Communities c ON p.communityID = c.communityID
            JOIN Users adm ON c.adminID = adm.userID
            WHERE p.communityID IN ({idList})";

        return FetchList(query);
    }

    public List<Post> GetPostExceptCommunityIDs(int[] communityIDs)
    {
        if (communityIDs.Length == 0) return new List<Post>();

        string idList = string.Join(",", communityIDs);
        string query = $@"
            SELECT p.*, 
                   u.username AS owner_username, u.email AS owner_email, u.avatarUrl AS owner_avatarUrl, u.bio AS owner_bio, u.status AS owner_status,
                   c.name AS community_name, c.description AS community_description,
                   adm.userID AS admin_userID, adm.username AS admin_username
            FROM Posts p
            JOIN Users u ON p.ownerID = u.userID
            JOIN Communities c ON p.communityID = c.communityID
            JOIN Users adm ON c.adminID = adm.userID
            WHERE p.communityID NOT IN ({idList})";

        return FetchList(query);
    }



    private List<Post> FetchList(string query)
    {
        var list = new List<Post>();
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);
        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read()) list.Add(MapReaderToPost(reader));
        return list;
    }

    private static Post MapReaderToPost(SqlDataReader reader)
    {
        var imageOrdinal = reader.GetOrdinal("image");

        var owner = new User
        {
            UserID = reader.GetInt32(reader.GetOrdinal("ownerID")),
            Username = reader.GetString(reader.GetOrdinal("owner_username")),
            Email = reader.GetString(reader.GetOrdinal("owner_email")),

            AvatarUrl = reader.IsDBNull(reader.GetOrdinal("owner_avatarUrl")) ? null : reader.GetString(reader.GetOrdinal("owner_avatarUrl")),
            Bio = reader.IsDBNull(reader.GetOrdinal("owner_bio")) ? null : reader.GetString(reader.GetOrdinal("owner_bio")),
            Status = reader.IsDBNull(reader.GetOrdinal("owner_status")) ? null : reader.GetString(reader.GetOrdinal("owner_status"))
        }; 

        var communityAdmin = new User
        {
            UserID = reader.GetInt32(reader.GetOrdinal("admin_userID")),
            Username = reader.GetString(reader.GetOrdinal("admin_username")),
        };


        var community = new Community
        {
            CommunityID = reader.GetInt32(reader.GetOrdinal("communityID")),
            Name = reader.GetString(reader.GetOrdinal("community_name")),
            Description = reader.GetString(reader.GetOrdinal("community_description")),
            Admin = communityAdmin,
        };

        return new Post
        {
            PostID = reader.GetInt32(reader.GetOrdinal("postID")),
            Title = reader.GetString(reader.GetOrdinal("title")),
            Description = reader.GetString(reader.GetOrdinal("description")),
            Image = reader.IsDBNull(imageOrdinal) ? null : (byte[])reader["image"],
            Score = reader.GetInt32(reader.GetOrdinal("score")),
            CommentsNumber = reader.GetInt32(reader.GetOrdinal("commentsNumber")),
            CreationTime = reader.GetDateTime(reader.GetOrdinal("creationTime")),
            Owner = owner,
            ParentCommunity = community
        };
    }
}