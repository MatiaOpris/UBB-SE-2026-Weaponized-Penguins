using Boards_WP.Data.Models;
using Boards_WP.ViewModels;

using Microsoft.UI.Xaml;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;

namespace Boards_WP.Views
{
    public sealed partial class CommentView : UserControl
    {
        public CommentViewModel ViewModel { get; set; }

        public static readonly DependencyProperty CommentDataProperty =
            DependencyProperty.Register("CommentData", typeof(Comment), typeof(CommentView),
                new PropertyMetadata(null, OnCommentDataChanged));

        public Comment CommentData
        {
            get => (Comment)GetValue(CommentDataProperty);
            set => SetValue(CommentDataProperty, value);
        }

        public CommentView() => this.InitializeComponent();

        private static void OnCommentDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CommentView control && e.NewValue is Comment comment)
            {
                control.ViewModel = new CommentViewModel(comment);
                control.ViewModel.ReplySubmitted = control.HandleReplySubmission;
                control.Bindings.Update();
            }
        }

        private void HandleReplySubmission(Comment parent, string text)
        {
            if (App.Current is App myApp && myApp.m_window.Content is FrameworkElement root)
            {
                var frame = root.FindName("ContentFrame") as Frame;

                if (frame?.Content is Pages.FullPostView postPage)
                {
                    var userSession = App.GetService<UserSession>();
                    var commentsService = App.GetService<Boards_WP.Data.Services.Interfaces.ICommentsService>();
                    var postsService = App.GetService<Boards_WP.Data.Services.Interfaces.IPostsService>();

                    var newReply = new Comment
                    {
                        ParentPost = postPage.ViewModel.CurrentPost,
                        ParentComment = parent,
                        Owner = userSession.CurrentUser,
                        Description = text,
                        Score = 0,
                        Indentation = parent.Indentation + 1
                    };

                    try
                    {
                        commentsService.AddComment(newReply);
                        postsService.IncreaseCommentsNumber(postPage.ViewModel.CurrentPost.PostID);


                       
                        postPage.ViewModel.Initialize(postPage.ViewModel.CurrentPost);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to reply: {ex.Message} \n {ex.StackTrace}");
                        throw;
                    }
                }
            }
        }

        public string FormatCommentDate(DateTime date) => date.ToString("g");
        public Thickness GetIndentMargin(int indentation) => new Thickness(indentation * 40, 0, 0, 10);
        public Visibility GetLineVisibility(int indentation) => indentation > 0 ? Visibility.Visible : Visibility.Collapsed;
    }
}