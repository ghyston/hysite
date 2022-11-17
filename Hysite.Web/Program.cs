using System.IO;
using System.Reflection;
using hySite;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
//TODO: add logger provider for logs into file
//loggerFactory.AddFile(logsPath + "/hysite-{Date}.log");


// Configure services
builder.Services.AddControllers();
builder.Services.AddRazorPages()
	.AddRazorPagesOptions(options => {
	options.Conventions.AddPageRoute("/Index", "");
	options.Conventions.AddPageRoute("/Post", "{postname}");
});

builder.Services.AddMediatR(Assembly.GetExecutingAssembly());

//TODO: this or app.UseHttpsRedirection ?
builder.Services.AddHttpsRedirection(options =>
{
	options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
});


//@todo: use one postsFileProvider
builder.Services.AddSingleton<IFileProvider>(builder.Environment.ContentRootFileProvider);
builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("db"));
builder.Services.AddScoped<IBlogPostRepository, BlogPostRepository>(); //Scoped has lifetime per request
builder.Services.AddScoped<IViewStatisticRepository, ViewStatisticRepository>(); //Scoped has lifetime per request
builder.Services.AddTransient<IGitRepository, GitRepository>();
builder.Services.AddTransient<IFileParserService, FileParserService>(); //Transient created each time
builder.Services.AddScoped<IRssFeedService, RssFeedService>();
builder.Services.AddSingleton<IVersionService, VersionService>();
builder.Services.AddTransient<IHandler<IncrementViewsHandlerRequest, IncrementViewsHandlerResponse>, IncrementViewsHandlerHandler>();

//builder.Services.BuildServiceProvider() //TODO: why do we need that?


var app = builder.Build();

// Configure app
 var logsPath = app.Configuration["LogsLocalPath"];
if (!Directory.Exists(logsPath))
	Directory.CreateDirectory(logsPath);


app.Logger.LogInformation("Staaaart UP!");

var versionService = app.Services.GetService<IVersionService>();

var version = versionService.GetCurrentGitSHA();
app.Logger.LogWarning($"git commit: {version}");

if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
}
else
{
	app.UseExceptionHandler("/Error");
}

var postsPath = app.Configuration["PostsLocalPath"];

var configParsed = bool.TryParse(app.Configuration["loadFromGit"], out bool loadFromGit);
if(configParsed && loadFromGit)
{
	if(Directory.Exists(postsPath))
		Directory.Delete(postsPath, recursive: true);

	//gitRepository.Clone();
}

// Temp
var directoryToCheck = "/app/cert/";
if(Directory.Exists(directoryToCheck))
{
	var files = Directory.GetFiles("/app/cert/");
	app.Logger.LogInformation("certs folder content:");
	foreach (var file in files)
		app.Logger.LogInformation(file);
}
app.Logger.LogInformation($"Kestrel path: {app.Configuration["Kestrel:Certificates:Default:Path"]}");
app.Logger.LogInformation($"Kestrel keypath: {app.Configuration["Kestrel:Certificates:Default:KeyPath"]}");
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

//TODO: use IHostService for that!
//var fileParser = app.Services.GetService<IFileParserService>();
//fileParser.ParseExistingFiles();


app.Run();