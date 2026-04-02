using System;
using System.Collections.ObjectModel;
using Boards_WP.Data.Models;
using Boards_WP.Data.Repositories.Interfaces;
using Boards_WP.Data.Services.Interfaces;
using Boards_WP.Views.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Boards_WP.ViewModels
{
    public partial class CreatePostViewModel : ObservableObject
    {
        private readonly IPostsService _postsService;
        private readonly INavigationService _navigationService;
        private readonly UserSession _userSession;
        private readonly ITagsRepository _tagsRepository;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(UploadPostCommand))]
        private string _postTitle = string.Empty;

        [ObservableProperty]
        private string _postDescription = string.Empty;

        [ObservableProperty]
        private string _tagsInput = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(UploadPostCommand))]
        private Category? _selectedCategory;

        [ObservableProperty]
        private byte[]? _postImage;

        public ObservableCollection<Category> AvailableCategories { get; } = new();

        public Community OriginCommunity { get; set; } = null!;

        public CreatePostViewModel(IPostsService postsService, INavigationService navigationService, UserSession userSession, ITagsRepository tagsRepository)
        {
            _postsService = postsService;
            _navigationService = navigationService;
            _userSession = userSession;
            _tagsRepository = tagsRepository;
            LoadCategories();
        }

        private void LoadCategories()
        {
            AvailableCategories.Clear();
            var categories = _tagsRepository.GetAllCategories();
            foreach (var c in categories) AvailableCategories.Add(c);
        }

        [RelayCommand(CanExecute = nameof(CanUploadPost))]
        private void UploadPost()
        {
            var newPost = new Post
            {
                Title = PostTitle,
                Description = PostDescription,
                ParentCommunity = OriginCommunity,
                Owner = _userSession.CurrentUser,
                Score = 0,
                Image = PostImage,
                CommentsNumber = 0,
                CreationTime = DateTime.Now
            };

            if (SelectedCategory != null)
            {
                var inputTags = TagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var createdTags = new System.Collections.Generic.List<Tag>();

                foreach(var tagName in inputTags)
                {
                    var tag = new Tag
                    {
                        TagName = tagName,
                        CategoryBelongingTo = SelectedCategory
                    };
                    _tagsRepository.AddTag(tag);
                    createdTags.Add(tag);
                }

                if (createdTags.Count == 0)
                {
                    var tag = new Tag { TagName = SelectedCategory.CategoryName, CategoryBelongingTo = SelectedCategory };
                    _tagsRepository.AddTag(tag);
                    createdTags.Add(tag);
                }

                newPost.Tags = createdTags;
            }

            _postsService.AddPost(newPost);

            _navigationService.NavigateTo(typeof(CommunityView), OriginCommunity);
        }

        [RelayCommand]
        private void Cancel()
        {
            if (_navigationService.CanGoBack)
            {
                _navigationService.GoBack();
            }
        }

        private bool CanUploadPost() => !string.IsNullOrWhiteSpace(PostTitle) && SelectedCategory != null;
    }
}
