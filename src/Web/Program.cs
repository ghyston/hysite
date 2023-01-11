using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using HySite.Application.Interfaces;
using HySite.Infrastructure;
using HySite.Web;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
//TODO: add logger provider for logs into file
//loggerFactory.AddFile(logsPath + "/hysite-{Date}.log");


// Configure services

//builder.Services.AddMediatR(Assembly.GetExecutingAssembly());


//builder.Services.AddSingleton<IFileProvider>(builder.Environment.ContentRootFileProvider);

builder.Services.AddWebPresentation();
builder.Services.AddInfrastructure();

//builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("db"));
//builder.Services.AddSingleton<IVersionService, VersionService>();

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

/*var configParsed = bool.TryParse(app.Configuration["loadFromGit"], out bool loadFromGit);
if(configParsed && loadFromGit)
{
	if(Directory.Exists(postsPath))
		Directory.Delete(postsPath, recursive: true);

	gitRepository.Clone();
}*/

// Temp
var fullchainPath = app.Configuration["Kestrel:Certificates:Default:Path"];
var privkeyPath = app.Configuration["Kestrel:Certificates:Default:KeyPath"];

if(File.Exists(fullchainPath))
	app.Logger.LogInformation($"file {fullchainPath} DOES exist!");
else
	app.Logger.LogInformation($"file {fullchainPath} does NOT exist!");

var directoryToCheck = "/app/cert/";
if(Directory.Exists(directoryToCheck))
{
	var files = Directory.GetFiles("/app/cert/");
	app.Logger.LogInformation("certs folder content:");
	foreach (var file in files)
		app.Logger.LogInformation(file);
}
app.Logger.LogInformation($"Kestrel path: {fullchainPath}");
app.Logger.LogInformation($"Kestrel keypath: {privkeyPath}");
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

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

//TODO: use IHostService for that!
//var fileParser = app.Services.GetService<IFileParserService>();
//fileParser.ParseExistingFiles();


app.Run();