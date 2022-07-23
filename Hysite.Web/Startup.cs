using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Serilog.Extensions.Logging.File;
using Markdig;
using System.Collections;

namespace hySite
{
    public class Startup
    {
        private IWebHostEnvironment _webHostingEnviroment;
        private IConfiguration _configuration;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;  
            _webHostingEnviroment = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddRazorPages()
                .AddRazorPagesOptions(options => {
                options.Conventions.AddPageRoute("/Index", "");
                options.Conventions.AddPageRoute("/Post", "{postname}");
            });

            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
            });

            //@todo: use one postsFileProvider
            var physicalProvider = _webHostingEnviroment.ContentRootFileProvider;

            services.AddSingleton<IFileProvider>(physicalProvider);
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("db"));
            services.AddScoped<IBlogPostRepository, BlogPostRepository>(); //Scoped has lifetime per request
            services.AddScoped<IViewStatisticRepository, ViewStatisticRepository>(); //Scoped has lifetime per request
            services.AddTransient<IGitRepository, GitRepository>();
            services.AddTransient<IFileParserService, FileParserService>(); //Transient created each time
            services.AddSingleton<IRssFeedService, RssFeedService>();
            services.AddSingleton<IVersionService, VersionService>();
            services.AddTransient<IHandler<IncrementViewsHandlerRequest, IncrementViewsHandlerResponse>, IncrementViewsHandlerHandler>();

            return services.BuildServiceProvider();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app, 
            IWebHostEnvironment environment,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            IGitRepository gitRepository,
            IVersionService versionService,
            IConfiguration configuration)
        {   
            var logsPath = configuration["LogsLocalPath"];
            if (!Directory.Exists(logsPath))
                Directory.CreateDirectory(logsPath);

            loggerFactory.AddFile(logsPath + "/hysite-{Date}.log");

            var logger = loggerFactory.CreateLogger("startup");
            logger.LogInformation("Staaaart UP!");
            var version = versionService.GetCurrentGitSHA();
            logger.LogWarning($"git commit: {version}");

            if (environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            var postsPath = configuration["PostsLocalPath"];

            var configParsed = bool.TryParse(configuration["loadFromGit"], out bool loadFromGit);
            if(configParsed && loadFromGit)
            {
                if(Directory.Exists(postsPath))
                    Directory.Delete(postsPath, recursive: true);

                //gitRepository.Clone();
            }

            // Temp
            var files = Directory.GetFiles("/app/cert/");
            logger.LogInformation("certs folder content:");
            foreach (var file in files)
            {
                logger.LogInformation(file);
            }
            logger.LogInformation($"Kestrel path: {configuration["Kestrel:Certificates:Default:Path"]}");
            logger.LogInformation($"Kestrel keypath: {configuration["Kestrel:Certificates:Default:KeyPath"]}");
            // End temp
            
            var postsFullPath = Path.Combine(Directory.GetCurrentDirectory(), postsPath);
            var imagesFullPth = Path.Combine(postsFullPath, "img");

            if(!Directory.Exists(postsFullPath))
                Directory.CreateDirectory(postsFullPath);

            if(!Directory.Exists(imagesFullPth))
                Directory.CreateDirectory(imagesFullPth);

            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions()
            {                
                FileProvider = new PhysicalFileProvider(postsFullPath),
                RequestPath = new PathString("")
            });
            app.UseStaticFiles(new StaticFileOptions()
            {                
                FileProvider = new PhysicalFileProvider(imagesFullPth),
                RequestPath = new PathString("")
            });

            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseEndpoints(endpoints => 
            {
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}"
                );
            });

            var fileParser = serviceProvider.GetService<IFileParserService>();
            fileParser.ParseExistingFiles();
        }
    }
}
