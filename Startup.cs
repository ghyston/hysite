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
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddRazorPagesOptions(options => {
                options.Conventions.AddPageRoute("/Index", "");
                options.Conventions.AddPageRoute("/Post", "{postname:alpha}");
            });
            
            //@todo: use one postsFileProvider
            var physicalProvider = _hostingEnviroment.ContentRootFileProvider;
            services.AddSingleton<IFileProvider>(physicalProvider);
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("db"));
            services.AddScoped<IBlogPostRepository, BlogPostRepository>(); //Scoped has lifetime per request
            services.AddTransient<IFileParserService, FileParserService>(); //Transient created each time
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IFileParserService fileParserService)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseStaticFiles(new StaticFileOptions()
            {                
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "posts")),
                RequestPath = new PathString("")
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            //var start = DateTime.Now;

            fileParserService.CreateDb();

            /*if(env.IsDevelopment())
            {
                var diff = (DateTime.Now - start).ToString();
                var count = db.BlogPosts.Count();
                Console.WriteLine($"Parsing {count} posts, took {diff} time");
            }*/
        }
    }
}
