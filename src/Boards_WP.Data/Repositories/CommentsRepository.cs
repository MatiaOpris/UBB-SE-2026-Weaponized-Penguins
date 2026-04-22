using System;
using System.Data;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Text;

using Boards_WP.Data.Models;
using Boards_WP.Data.Repositories.Interfaces;

namespace Boards_WP.Data.Repositories;

public class CommentsRepository : ICommentsRepository
{
    private readonly String _connectionString;

    public CommentsRepository(String connectionString)
    {
        _connectionString = connectionString;
    }

    public void AddComment(Comment comment)
    {
        const string query = @"
            INSERT INTO Comments (postID, parentID, ownerID, description, score, creationTime, indentation, isDeleted)
            VALUES (@postID, @parentID, @ownerID, @description, @score, @creationTime, @indentation, @isDeleted);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);

        command.Parameters.Add("@postID", SqlDbType.Int).Value = comment.ParentPost.PostID;
        command.Parameters.Add("@parentID", SqlDbType.Int).Value = comment.ParentComment != null && comment.ParentComment.CommentID != 0 ? (object)comment.ParentComment.CommentID : DBNull.Value;
        command.Parameters.Add("@ownerID", SqlDbType.Int).Value = comment.Owner.UserID;
        command.Parameters.Add("@description", SqlDbType.NVarChar, 618).Value = comment.Description;
        command.Parameters.Add("@score", SqlDbType.Int).Value = comment.Score;
        command.Parameters.Add("@creationTime", SqlDbType.DateTime).Value = comment.CreationTime;
        command.Parameters.Add("@indentation", SqlDbType.Int).Value = comment.Indentation;
        command.Parameters.Add("@isDeleted", SqlDbType.Bit).Value = comment.IsDeleted;

        connection.Open();
        comment.CommentID = (int)command.ExecuteScalar();
    }

    public void SoftDeleteComment(int commentID)
    {
        const string query = @"
            UPDATE Comments 
            SET isDeleted = 1, description = '[deleted]' 
            WHERE commentID = @commentID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);

        command.Parameters.Add("@commentID", SqlDbType.Int).Value = commentID;

        connection.Open();
        command.ExecuteNonQuery();
    }

    public void IncreaseScore(Comment comment)
    {
        const string query = "UPDATE Comments SET score = score + 1 WHERE commentID = @commentID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);

        command.Parameters.Add("@commentID", SqlDbType.Int).Value = comment.CommentID;

        connection.Open();
        command.ExecuteNonQuery();

        comment.Score += 1;
    }

    public void DecrementCommentScore(Comment comment)
    {
        const string query = "UPDATE Comments SET score = score - 1 WHERE commentID = @commentID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);

        command.Parameters.Add("@commentID", SqlDbType.Int).Value = comment.CommentID;

        connection.Open();
        command.ExecuteNonQuery();

        comment.Score -= 1;
    }

    public List<Comment> GetCommentsByPostID(int postID, int userID)
    {
        var comments = new List<Comment>();

        const string query = @"
            SELECT 
                c.commentID, 
                c.postID, 
                c.parentID, 
                c.ownerID, 
                c.description, 
                c.score, 
                c.creationTime, 
                c.indentation, 
                c.isDeleted,
                u.username,
                cv.vote
            FROM Comments c
            INNER JOIN Users u ON c.ownerID = u.userID
            LEFT JOIN CommentsViews cv ON c.commentID = cv.commentID AND cv.userID = @userID
            WHERE c.postID = @postID
            ORDER BY c.creationTime ASC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);

        command.Parameters.Add("@postID", SqlDbType.Int).Value = postID;
        command.Parameters.Add("@userID", SqlDbType.Int).Value = userID;

        connection.Open();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            int commentID = reader.GetInt32(0);
            int pID = reader.GetInt32(1);
            int? parentID = reader.IsDBNull(2) ? null : reader.GetInt32(2);
            int ownerID = reader.GetInt32(3);
            string description = reader.GetString(4);
            int score = reader.GetInt32(5);
            DateTime creationTime = reader.GetDateTime(6);
            int indentation = reader.GetInt32(7);
            bool isDeleted = reader.GetBoolean(8);
            string username = reader.GetString(9);

            int? voteVal = reader.IsDBNull(10) ? null : reader.GetInt32(10);
            VoteType vote = voteVal.HasValue ? (VoteType)voteVal.Value : VoteType.None;

            var comment = new Comment
            {
                CommentID = commentID,
                ParentPost = new Post { PostID = pID },
                ParentComment = parentID.HasValue ? new Comment { CommentID = parentID.Value } : null,
                Owner = new User { UserID = ownerID, Username = username },
                Description = description,
                Score = score,
                CreationTime = creationTime,
                Indentation = indentation,
                IsDeleted = isDeleted,
                UserCurrentVote = vote
            };

            comments.Add(comment);
        }

        return comments;
    }

    public void UpsertUserCommentVote(int commentID, int currentUserID, VoteType vote)
    {
        const string query = @"
            MERGE INTO CommentsViews AS target
            USING (VALUES (@userID, @commentID, @vote)) AS source (userID, commentID, vote)
            ON target.userID = source.userID AND target.commentID = source.commentID
            WHEN MATCHED THEN
                UPDATE SET vote = source.vote
            WHEN NOT MATCHED THEN
                INSERT (userID, commentID, vote)
                VALUES (source.userID, source.commentID, source.vote);";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);

        command.Parameters.Add("@userID", SqlDbType.Int).Value = currentUserID;
        command.Parameters.Add("@commentID", SqlDbType.Int).Value = commentID;
        command.Parameters.Add("@vote", SqlDbType.Int).Value = (int)vote;

        connection.Open();
        command.ExecuteNonQuery();
    }
}
