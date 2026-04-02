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
                    var newReply = new Comment
                    {
                        CommentID = new Random().Next(1000, 9999),
                        Owner = new User { Username = "@Me" },
                        Description = text,
                        Score = 0,
                        CreationTime = DateTime.Now,
                        Indentation = parent.Indentation + 1
                    };

                    int index = postPage.ViewModel.PostComments.IndexOf(parent);
                    postPage.ViewModel.PostComments.Insert(index + 1, newReply);
                }
            }
        }

        public string FormatCommentDate(DateTime date) => date.ToString("g");
        public Thickness GetIndentMargin(int indentation) => new Thickness(indentation * 40, 0, 0, 10);
        public Visibility GetLineVisibility(int indentation) => indentation > 0 ? Visibility.Visible : Visibility.Collapsed;
    }
}