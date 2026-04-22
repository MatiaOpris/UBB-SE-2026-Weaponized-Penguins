using Boards_WP.Views.Pages;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Boards_WP.ViewModels
{
    public partial class CreatePostViewModel : ObservableObject
    {
        private readonly IPostsService postsService;
        private readonly INavigationService navigationService;
        private readonly UserSession userSession;
        private readonly ITagsRepository tagsRepository;
        private MainViewModel mainViewModel;

        public MainViewModel MainViewModel => mainViewModel;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(UploadPostCommand))]
        private string postTitle = string.Empty;

        [ObservableProperty]
        private string postDescription = string.Empty;

        [ObservableProperty]
        private string tagsInput = string.Empty;

        [ObservableProperty]
        private string currentTagText = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(UploadPostCommand))]
        private Category? selectedCategory;

        private byte[] postImage = null!;

        [global::System.Diagnostics.CodeAnalysis.MaybeNull]
        public byte[] PostImage
        {
            get => postImage;
            set => SetProperty(ref postImage, value!);
        }

        public ObservableCollection<Category> AvailableCategories { get; } = new ();
        public ObservableCollection<Tag> AddedTags { get; } = new ();

        public Community OriginCommunity { get; set; } = null!;

        public CreatePostViewModel(IPostsService postsService, INavigationService navigationService, UserSession userSession, ITagsRepository tagsRepository)
        {
            mainViewModel = App.GetService<MainViewModel>();
            this.postsService = postsService;
            this.navigationService = navigationService;
            this.userSession = userSession;
            this.tagsRepository = tagsRepository;
            LoadCategories();
        }

        private void LoadCategories()
        {
            AvailableCategories.Clear();
            var categories = tagsRepository.GetAllCategories();
            foreach (var c in categories)
            {
                AvailableCategories.Add(c);
            }
        }

        [RelayCommand]
        private void AddTag()
        {
            if (string.IsNullOrWhiteSpace(CurrentTagText) || SelectedCategory == null)
            {
                return;
            }

            var tag = new Tag { TagName = CurrentTagText.Trim(), CategoryBelongingTo = SelectedCategory };
            if (!AddedTags.Contains(tag))
            {
                AddedTags.Add(tag);
            }

            CurrentTagText = string.Empty;
        }

        [RelayCommand]
        private void RemoveTag(Tag tag)
        {
            if (tag != null && AddedTags.Contains(tag))
            {
                AddedTags.Remove(tag);
            }
        }

        [RelayCommand(CanExecute = nameof(CanUploadPost))]
        private void UploadPost()
        {
            var newPost = new Post
            {
                Title = PostTitle,
                Description = PostDescription,
                ParentCommunity = OriginCommunity,
                Owner = userSession.CurrentUser,
                Score = 0,
                Image = PostImage,
                CommentsNumber = 0,
                CreationTime = DateTime.Now
            };

            if (SelectedCategory != null)
            {
                var inputTags = TagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var createdTags = new System.Collections.Generic.List<Tag>();

                foreach (var tagName in inputTags)
                {
                    var tag = new Tag
                    {
                        TagName = tagName,
                        CategoryBelongingTo = SelectedCategory
                    };
                    tagsRepository.AddTag(tag);
                    createdTags.Add(tag);
                }

                if (createdTags.Count == 0)
                {
                    var tag = new Tag { TagName = SelectedCategory.CategoryName, CategoryBelongingTo = SelectedCategory };
                    tagsRepository.AddTag(tag);
                    createdTags.Add(tag);
                }

                newPost.Tags = createdTags;
            }

            postsService.AddPost(newPost);

            navigationService.NavigateTo(typeof(CommunityView), OriginCommunity);
        }

        [RelayCommand]
        private void Cancel()
        {
            if (navigationService.CanGoBack)
            {
                navigationService.GoBack();
            }
        }

        private bool CanUploadPost() => !string.IsNullOrWhiteSpace(PostTitle) && SelectedCategory != null;
    }
}
