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
                Owner = userSession.CurrentUser, // Grabbing user correctly via session
                Score = 0,
                Image = PostImage,
                CommentsNumber = 0,
                CreationTime = DateTime.Now
            };

            if (SelectedCategory != null)
            {
                var createdTags = new System.Collections.Generic.List<Tag>();

                // We now properly iterate over the actual AddedTags collection
                foreach (var tag in AddedTags)
                {
                    tagsRepository.AddTag(tag);
                    createdTags.Add(tag);
                }

                // Make sure at least one default tag exists reflecting their Category preference
                if (createdTags.Count == 0)
                {
                    var defaultCategoryTag = new Tag { TagName = SelectedCategory.CategoryName, CategoryBelongingTo = SelectedCategory };
                    tagsRepository.AddTag(defaultCategoryTag);
                    createdTags.Add(defaultCategoryTag);
                }

                newPost.Tags = createdTags;
            }

            // Properly commit changes to standard repository
            postsService.CreatePost(newPost);

            // Cleanup fields after successful post
            PostTitle = string.Empty;
            PostDescription = string.Empty;
            CurrentTagText = string.Empty;
            TagsInput = string.Empty;
            AddedTags.Clear();
            SelectedCategory = null;
            PostImage = null;

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
