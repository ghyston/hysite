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

namespace hysite
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
            FindPosts();
        }

        public void FindPosts()
        {
            
            IFileProvider fileProvider = _hostingEnviroment.ContentRootFileProvider;
            var contents = fileProvider.GetDirectoryContents("posts");

            Console.WriteLine("Files:");
            var postFiles = contents.Where(f => f.Name.EndsWith(".md") && !f.IsDirectory).OrderBy(f => f.LastModified);
            foreach(var fileInfo in postFiles)
            {
                Console.WriteLine($"{fileInfo.Name} {fileInfo.LastModified.ToString()}");
            }
            Console.WriteLine("Thats all");
        }
    }
}
