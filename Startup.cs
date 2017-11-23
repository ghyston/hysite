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

            var start = DateTime.Now;
            createDb(fileProvider, db);

            if(env.IsDevelopment())
            {
                var diff = (DateTime.Now - start).ToString();
                var count = db.BlogPosts.Count();
                Console.WriteLine($"Parsing {count} posts, took {diff} time");
            }
        }

        private void createDb(IFileProvider fileProvider, AppDbContext db)
        {
            IDirectoryContents contents = fileProvider.GetDirectoryContents("posts");
            IEnumerable<IFileInfo> files = contents.Where(f => f.Name.EndsWith(".md") && !f.IsDirectory).OrderBy(f => f.LastModified);

            foreach(var fileInfo in files)
            {
                AddFile(fileInfo, db);
            }
            db.SaveChanges();
        }

        //@todo: parallel execution for all these files?
        private void AddFile(IFileInfo fileInfo, AppDbContext db)
        {
            var streamReader = new StreamReader(fileInfo.CreateReadStream());
                var title = streamReader.ReadLine();
                var timeStr = streamReader.ReadLine();
                DateTime postCreated = DateTime.Parse(timeStr); //@todo: do format provider, YYYY/mm/dd HH:mm
                var unusedMetaDataLine = streamReader.ReadLine();
                while(unusedMetaDataLine != "@@@")
                {
                    unusedMetaDataLine = streamReader.ReadLine();
                }

                var mdContent = streamReader.ReadToEnd();
                var htmlContent = Markdown.ToHtml(mdContent);

                BlogPost post = new BlogPost()
                {
                    Title = title,
                    MdContent = mdContent,
                    HtmlContent = htmlContent,
                    Created = postCreated
                };
                db.BlogPosts.Add(post);
        }
    }
}
