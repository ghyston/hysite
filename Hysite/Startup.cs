using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.EntityFrameworkCore;
using Markdig;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging.File;

namespace hySite
{
    public class Startup
    {
        private IHostingEnvironment _hostingEnviroment;
        private IConfiguration _configuration;


        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            _configuration = configuration;  
            _hostingEnviroment = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddRazorPagesOptions(options => {
                options.Conventions.AddPageRoute("/Index", "");
                options.Conventions.AddPageRoute("/Post", "{postname}");
            });

            //@todo: use one postsFileProvider
            var physicalProvider = _hostingEnviroment.ContentRootFileProvider;

            services.AddSingleton<IFileProvider>(physicalProvider);
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("db"));
            services.AddScoped<IBlogPostRepository, BlogPostRepository>(); //Scoped has lifetime per request
            services.AddScoped<IViewStatisticRepository, ViewStatisticRepository>(); //Scoped has lifetime per request
            services.AddTransient<IGitRepository, GitRepository>();
            services.AddTransient<IFileParserService, FileParserService>(); //Transient created each time
            services.AddSingleton<IFileWatcherSingleton, FileWatcherService>();
            
            services.AddTransient<IHandler<OnNewFileRequest, OnNewFileResponse>, OnNewFileHandler>();
            services.AddTransient<IHandler<OnFileChangedRequest, OnFileChangedResponse>, OnFileChangedHandler>();
            services.AddTransient<IHandler<OnFileRenamedRequest, OnFileRenamedResponse>, OnFileRenamedHandler>();
            services.AddTransient<IHandler<OnFileDeletedRequest, OnFileDeletedResponse>, OnFileDeletedHandler>();
            services.AddTransient<IHandler<IncrementViewsHandlerRequest, IncrementViewsHandlerResponse>, IncrementViewsHandlerHandler>();

            return services.BuildServiceProvider();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app, 
            IHostingEnvironment env, 
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            IGitRepository gitRepository,
            IConfiguration configuration)
        {
            var logsPath = configuration["LogsLocalPath"];
            if (!Directory.Exists(logsPath))
                Directory.CreateDirectory(logsPath);

            loggerFactory.AddFile(logsPath + "/hysite-{Date}.log");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            var postsPath = configuration["PostsLocalPath"];
            if(configuration["loadFromGit"].Equals("true"))
            {
                if (!Directory.Exists(postsPath))
                    Directory.CreateDirectory(postsPath);
                else
                    Directory.Delete(postsPath, recursive: true);
                gitRepository.Clone();
            }
            
            var postsFullPath = Path.Combine(Directory.GetCurrentDirectory(), postsPath);
            var imagesFullPth = Path.Combine(postsFullPath, "img");

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

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            var fileParser = serviceProvider.GetService<IFileParserService>();
            fileParser.ParseExistingFiles();            
        }
    }
}
