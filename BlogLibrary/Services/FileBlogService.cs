using Microsoft.AspNetCore.Http;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace BlogLibrary.Services;
public class FileBlogService : IBlogService
{
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
    public async Task SavePost(Post post)
    {
        if (post is null)
        {
            throw new ArgumentNullException(nameof(post));
        }

        var filePath = this.GetFilePath(post);
    }

    private string GetFilePath(Post post) => Path.Combine(this.folder, $"{post.ID}.xml");
}
