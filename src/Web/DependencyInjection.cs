using System.IO;
using System.Threading.Tasks;
using Hysite.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace HySite.Web;

public static class DependencyInjection
{
    public static void AddWebPresentation(this IServiceCollection services)
    {
        services.AddHostedService<StartupService>();

        services.AddControllers();
        services.AddRazorPages()
            .AddRazorPagesOptions(options =>
            {
                options.Conventions.AddPageRoute("/Index", "");
                options.Conventions.AddPageRoute("/Post", "{postname}");
            });

        services.AddHttpsRedirection(options =>
        {
            options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
        });
    }

    public static void SetupRouting(this WebApplication app, params string[] pathesForStaticFiles)
    {
        if (app.Environment.IsDevelopment())
            app.UseDeveloperExceptionPage();
        else
            app.UseExceptionHandler("/Error");

        app.UseStatusCodePages(handler: async statusCodeContext =>
        {
            var redirectUrl = statusCodeContext.HttpContext.Response.StatusCode switch
            {
                404 => "/LostAndNotFound",
                _ => "/Error"
            };

            statusCodeContext.HttpContext.Response.Redirect(redirectUrl);

            await Task.CompletedTask;
        });

        app.UseStaticFiles();

        foreach (var path in pathesForStaticFiles)
        {
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(path),
                RequestPath = new PathString("")
            });
        }

        app.UseRouting();
        app.UseHttpsRedirection();

        app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();
    }
}