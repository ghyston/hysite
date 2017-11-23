using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.EntityFrameworkCore;

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
            services.AddMvc();

            var physicalProvider = _hostingEnviroment.ContentRootFileProvider;
            services.AddSingleton<IFileProvider>(physicalProvider);
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("db"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, AppDbContext db, IFileProvider fileProvider)
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

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            var posts = findPosts(fileProvider);
            createDb(posts, db);
        }

        private IEnumerable<IFileInfo> findPosts(IFileProvider fileProvider)
        {
            var contents = fileProvider.GetDirectoryContents("posts");

            Console.WriteLine("Files:");
            var postFiles = contents.Where(f => f.Name.EndsWith(".md") && !f.IsDirectory).OrderBy(f => f.LastModified);
            foreach(var fileInfo in postFiles)
            {
                Console.WriteLine($"{fileInfo.Name} {fileInfo.LastModified.ToString()}");
            }
            Console.WriteLine("Thats all");
            return postFiles;
        }

        private void createDb(IEnumerable<IFileInfo> files, AppDbContext db)
        {
            foreach(var fileInfo in files)
            {
                //@todo: first line: Title
                //@todo: second line: Timestamp
                // rest is mdContent

                var mdContent = new StreamReader(fileInfo.CreateReadStream()).ReadToEnd(); //@todo: do it async!
                var htmlContent = mdContent; //@todo: render to html!

                BlogPost post = new BlogPost()
                {
                    Title = fileInfo.Name,
                    MdContent = mdContent,
                    HtmlContent = htmlContent,
                    Created = fileInfo.LastModified.DateTime
                };
                db.BlogPosts.Add(post);
                //Console.WriteLine($"{fileInfo.Name} {fileInfo.LastModified.ToString()}");
            }
            db.SaveChanges();

        }
    }
}
