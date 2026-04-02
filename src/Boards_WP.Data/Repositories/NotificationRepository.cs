using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

using Boards_WP.Data.Models;
using Boards_WP.Data.Repositories.Interfaces;

namespace Boards_WP.Data.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly String _connectionString;

        public NotificationRepository(String connectionString)
        {
            _connectionString = connectionString;
        }

        public int AddNotification(Notification notification)
        {
            const string query = @"
                INSERT INTO Notifications (creationTime, postID, receiverID, actorID, actionType, isRead)
                OUTPUT INSERTED.notificationID
                VALUES (@CreationTime, @PostID, @ReceiverID, @ActorID, @ActionType, @isRead)";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@CreationTime", SqlDbType.DateTime).Value = notification.CreationTime;
            command.Parameters.Add("@PostID", SqlDbType.Int).Value = notification.RelatedPost.PostID;
            command.Parameters.Add("@ReceiverID", SqlDbType.Int).Value = notification.Receiver.UserID;
            command.Parameters.Add("@ActorID", SqlDbType.Int).Value = notification.Actor.UserID;
            command.Parameters.Add("@ActionType", SqlDbType.Int).Value = (int)notification.ActionType;
            command.Parameters.Add("@isRead", SqlDbType.Bit).Value = notification.IsRead;

            connection.Open();
            return (int)command.ExecuteScalar()!;
        }

        public void MarkNotificationAsRead(int notificationId)
        {
            const string query = @"
                UPDATE Notifications
                SET isRead = 1
                WHERE notificationID = @NotificationID";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@NotificationID", SqlDbType.Int).Value = notificationId;

            connection.Open();
            command.ExecuteNonQuery();
        }

        public List<Notification> GetNotificationsByUserId(int userId)
        {
            const string query = @"
                SELECT
                    n.notificationID,
                    n.creationTime,
                    n.actionType,
                    n.isRead,

                    receiver.userID       AS receiver_userID,
                    receiver.username     AS receiver_username,
                    receiver.email        AS receiver_email,
                    receiver.avatarUrl    AS receiver_avatarUrl,
                    receiver.bio          AS receiver_bio,
                    receiver.status       AS receiver_status,

                    actor.userID          AS actor_userID,
                    actor.username        AS actor_username,
                    actor.email           AS actor_email,
                    actor.avatarUrl       AS actor_avatarUrl,
                    actor.bio             AS actor_bio,
                    actor.status          AS actor_status,

                    p.postID,
                    p.title               AS postTitle
                FROM Notifications n
                JOIN Users receiver ON n.receiverID = receiver.userID
                JOIN Users actor    ON n.actorID    = actor.userID
                LEFT JOIN Posts p          ON n.RelatedPostID     = p.postID
                WHERE n.receiverID = @UserID
                ORDER BY n.creationTime DESC";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;

            connection.Open();
            using var reader = command.ExecuteReader();

            var notifications = new List<Notification>();
            while (reader.Read())
                notifications.Add(MapNotification(reader));

            return notifications;
        }

        private static Notification MapNotification(SqlDataReader reader)
        {
            var postIdOrdinal = reader.GetOrdinal("postID");
            var postTitleOrdinal = reader.GetOrdinal("postTitle");

            var receiver = new User
            {
                UserID = reader.GetInt32(reader.GetOrdinal("receiver_userID")),
                Username = reader.GetString(reader.GetOrdinal("receiver_username")),
                Email = reader.GetString(reader.GetOrdinal("receiver_email")),
                AvatarUrl = reader.IsDBNull(reader.GetOrdinal("receiver_avatarUrl")) ? null : reader.GetString(reader.GetOrdinal("receiver_avatarUrl")),
                Bio = reader.IsDBNull(reader.GetOrdinal("receiver_bio")) ? null : reader.GetString(reader.GetOrdinal("receiver_bio")),
                Status = reader.IsDBNull(reader.GetOrdinal("receiver_status")) ? null : reader.GetString(reader.GetOrdinal("receiver_status")),
            };

            var actor = new User
            {
                UserID = reader.GetInt32(reader.GetOrdinal("actor_userID")),
                Username = reader.GetString(reader.GetOrdinal("actor_username")),
                Email = reader.GetString(reader.GetOrdinal("actor_email")),
                AvatarUrl = reader.IsDBNull(reader.GetOrdinal("actor_avatarUrl")) ? null : reader.GetString(reader.GetOrdinal("actor_avatarUrl")),
                Bio = reader.IsDBNull(reader.GetOrdinal("actor_bio")) ? null : reader.GetString(reader.GetOrdinal("actor_bio")),
                Status = reader.IsDBNull(reader.GetOrdinal("actor_status")) ? null : reader.GetString(reader.GetOrdinal("actor_status")),
            };

            return new Notification
            {
                NotificationID = reader.GetInt32(reader.GetOrdinal("notificationID")),
                CreationTime = reader.GetDateTime(reader.GetOrdinal("creationTime")),
                ActionType = (NotificationType)reader.GetInt32(reader.GetOrdinal("actionType")),
                IsRead = reader.GetBoolean(reader.GetOrdinal("isRead")),
                Receiver = receiver,
                Actor = actor,
                RelatedPost = reader.IsDBNull(postIdOrdinal) ? null : new Post
                {
                    PostID = reader.GetInt32(postIdOrdinal),
                    Title = reader.GetString(postTitleOrdinal),
                    Owner = null!,
                    ParentCommunity = null!
                },
            };
        }
    }
}