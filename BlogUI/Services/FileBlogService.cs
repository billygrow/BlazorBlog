using BlogUI.Models;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BlogUI.Services;
public class FileBlogService : IBlogService
{
    const string FILES = "files";

    private const string POSTS = "Posts";

    private readonly List<Post> cache = new List<Post>();

    private readonly IHttpContextAccessor contextAccessor;

    private readonly string folder;
    public FileBlogService(IWebHostEnvironment env, IHttpContextAccessor contextAccessor)
    {
        if (env is null)
        {
            throw new ArgumentNullException(nameof(env));
        }

        this.folder = Path.Combine(env.WebRootPath, POSTS);
        this.contextAccessor = contextAccessor;

        this.Initialize();
    }

    protected bool IsAdmin() => this.contextAccessor.HttpContext?.User?.Identity.IsAuthenticated == true;

    public virtual IAsyncEnumerable<Post> GetPosts()
    {
        var isAdmin = true; //this.IsAdmin();

        var posts = this.cache
            .Where(p => p.PublishDate <= DateTime.UtcNow && (p.IsPublished || isAdmin)).ToAsyncEnumerable();

        return posts;
    }

    public virtual IAsyncEnumerable<Post> GetPosts(int count, int skip = 0)
    {
        var isAdmin = true; //this.IsAdmin();

        var posts = this.cache
            .Where(p => p.PublishDate <= DateTime.UtcNow && (p.IsPublished || isAdmin))
            .Skip(skip)
            .Take(count)
            .ToAsyncEnumerable();

        return posts;
    }
    public async Task SavePost(Post post)
    {
        if (post is null)
        {
            throw new ArgumentNullException(nameof(post));
        }

        var filePath = this.GetFilePath(post);

        var jsonString = JsonSerializer.Serialize(post);

        await File.WriteAllTextAsync(filePath, jsonString, CancellationToken.None);

        if (!this.cache.Contains(post))
        {
            this.cache.Add(post);
            this.SortCache();
        }
    }

    private void Initialize()
    {
        this.LoadPosts();
        this.SortCache();
    }
    private void LoadPosts()
    {
        if (!Directory.Exists(this.folder))
        {
            Directory.CreateDirectory(this.folder);
        }

        foreach (var file in Directory.EnumerateFiles(this.folder, "*.json", SearchOption.TopDirectoryOnly))
        {
            var jsonString = File.ReadAllText(file);

            var post = JsonSerializer.Deserialize<Post>(jsonString);

            this.cache.Add(post);
        }
    }

    protected void SortCache() => this.cache.Sort((p1, p2) => p2.PublishDate.CompareTo(p1.PublishDate));

    private string GetFilePath(Post post) => Path.Combine(this.folder, $"{post.ID}.json");

    public Task DeletePost(Post post)
    {
        if (post is null)
        {
            throw new ArgumentNullException(nameof(post));
        }

        var filePath = this.GetFilePath(post);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        if (this.cache.Contains(post))
        {
            this.cache.Remove(post);
        }

        return Task.CompletedTask;
    }

        [SuppressMessage(
            "Globalization",
            "CA1308:Normalize strings to uppercase",
            Justification = "Consumer preference.")]
        public virtual IAsyncEnumerable<string> GetCategories()
        {
                var isAdmin = true; // this.IsAdmin();

        return this.cache
                .Where(p => p.IsPublished || isAdmin)
                .SelectMany(post => post.Categories)
                .Select(cat => cat.ToLowerInvariant())
                .Distinct()
                .ToAsyncEnumerable();
        }

    [SuppressMessage(
            "Globalization",
            "CA1308:Normalize strings to uppercase",
            Justification = "Consumer preference.")]
    public virtual IAsyncEnumerable<string> GetTags()
    {
        var isAdmin = true; // this.IsAdmin();

        return this.cache
            .Where(p => p.IsPublished || isAdmin)
            .SelectMany(post => post.Tags)
            .Select(tag => tag.ToLowerInvariant())
            .Distinct()
            .ToAsyncEnumerable();
    }

    public virtual Task<Post> GetPostById(string id)
    {
        var isAdmin = true; // this.IsAdmin();
        var post = this.cache.FirstOrDefault(p => p.ID.Equals(id, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(
            post is null || post.PublishDate > DateTime.UtcNow || (!post.IsPublished && !isAdmin)
            ? null
            : post);
    }

    public virtual Task<Post> GetPostBySlug(string slug)
    {
        var isAdmin = true; // this.IsAdmin();
        var post = this.cache.FirstOrDefault(p => p.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(
            post is null || post.PublishDate > DateTime.UtcNow || (!post.IsPublished && !isAdmin)
            ? null
            : post);
    }

    public virtual IAsyncEnumerable<Post> GetPostsByCategory(string category)
    {
        var isAdmin = true; //this.IsAdmin();

        var posts = from p in this.cache
                    where p.PublishDate <= DateTime.UtcNow && (p.IsPublished || isAdmin)
                    where p.Categories.Contains(category, StringComparer.OrdinalIgnoreCase)
                    select p;

        return posts.ToAsyncEnumerable();
    }

    public IAsyncEnumerable<Post> GetPostsByTag(string tag)
    {
        var isAdmin = true; // this.IsAdmin();

        var posts = from p in this.cache
                    where p.PublishDate <= DateTime.UtcNow && (p.IsPublished || isAdmin)
                    where p.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)
                    select p;

        return posts.ToAsyncEnumerable();
    }

    public async Task<string> SaveFile(byte[] bytes, string fileName, string? suffix = null)
    {
        if (bytes is null)
        {
            throw new ArgumentNullException(nameof(bytes));
        }

        suffix = CleanFromInvalidChars(suffix ?? DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture));

        var ext = Path.GetExtension(fileName);
        var name = CleanFromInvalidChars(Path.GetFileNameWithoutExtension(fileName));

        var fileNameWithSuffix = $"{name}_{suffix}{ext}";

        var absolute = Path.Combine(this.folder, FILES, fileNameWithSuffix);
        var dir = Path.GetDirectoryName(absolute);

        Directory.CreateDirectory(dir);
        using (var writer = new FileStream(absolute, FileMode.CreateNew))
        {
            await writer.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
        }

        return $"/{POSTS}/{FILES}/{fileNameWithSuffix}";
    }

    private static string CleanFromInvalidChars(string input)
    {
        // ToDo: what we are doing here if we switch the blog from windows to unix system or
        // vice versa? we should remove all invalid chars for both systems

        var regexSearch = Regex.Escape(new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()));
        var r = new Regex($"[{regexSearch}]");
        return r.Replace(input, string.Empty);
    }

    private static string FormatDateTime(DateTime dateTime)
    {
        const string UTC = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'";

        return dateTime.Kind == DateTimeKind.Utc
            ? dateTime.ToString(UTC, CultureInfo.InvariantCulture)
            : dateTime.ToUniversalTime().ToString(UTC, CultureInfo.InvariantCulture);
    }
}
