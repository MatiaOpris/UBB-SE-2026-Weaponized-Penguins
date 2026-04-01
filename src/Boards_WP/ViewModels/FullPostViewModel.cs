using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Collections.ObjectModel;
using System.Collections.Generic;
using System;

using Boards_WP.Data.Models;

namespace Boards_WP.ViewModels
{
    public partial class FullPostViewModel : ObservableObject
    {
        [ObservableProperty]
        private Post _currentPost;

        [ObservableProperty]
        private string _newCommentText;

        public ObservableCollection<Comment> PostComments { get; } = new ObservableCollection<Comment>();

        public FullPostViewModel() { }

        public void LoadPost(Post post)
        {
            _currentPost = post;
            LoadMockComments();
        }

        private void LoadMockComments()
        {
            var hardcodedComments = new List<Comment>
            {
                new Comment { CommentID = 1, Owner = new User { Username = "@Alexandra" }, Description = "Lorem ipsum...", Score = 15, CreationTime = DateTime.Now.AddHours(-2), Indentation = 0 },
                new Comment { CommentID = 2, Owner = new User { Username = "@BerceaMatei" }, Description = "Short comment.", Score = 8, CreationTime = DateTime.Now.AddHours(-1), Indentation = 1 },
                new Comment { CommentID = 3, Owner = new User { Username = "@RazvanBerbecar" }, Description = "Longer comment text here...", Score = 12, CreationTime = DateTime.Now.AddMinutes(-30), Indentation = 2 },
                new Comment { CommentID = 4, Owner = new User { Username = "@BeneIonut" }, Description = "Perspiciatis unde omnis...", Score = 2, CreationTime = DateTime.Now.AddMinutes(-10), Indentation = 0 }
            };

            PostComments.Clear();
            foreach (var c in hardcodedComments) PostComments.Add(c);
        }

        [RelayCommand]
        private void PostComment()
        {
            if (string.IsNullOrWhiteSpace(NewCommentText)) return;

            var newComment = new Comment
            {
                CommentID = new Random().Next(1000, 9999),
                ParentPost = CurrentPost,
                Owner = new User { Username = "@Me" },
                Description = NewCommentText,
                Score = 0,
                CreationTime = DateTime.Now,
                Indentation = 0
            };

            PostComments.Insert(0, newComment);

            if (CurrentPost != null)
            {
                CurrentPost.CommentsNumber++;
                OnPropertyChanged(nameof(CurrentPost));
            }

            NewCommentText = string.Empty;
        }
    }
}