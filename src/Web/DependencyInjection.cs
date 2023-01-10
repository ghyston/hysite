using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;

namespace HySite.Web;

public static class DependencyInjection
{
    public static void AddWebPresentation(this IServiceCollection services)
    {
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
}