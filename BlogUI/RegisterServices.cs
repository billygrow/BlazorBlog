using BlogUI.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlogUI;
public static class RegisterServices
{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();
        builder.Services.AddControllers();
        builder.Services.AddMemoryCache();

        builder.Services.AddSingleton<IBlogService, FileBlogService>();
        builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

    }
}

