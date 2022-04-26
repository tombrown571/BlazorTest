using System.Diagnostics;
using System.Net;
using System.Security.Principal;
using System.Text;

using BlazorWebAssemblyApp;
using BlazorWebAssemblyApp.Services;
using FP.MissingLink;
//using FP.ApiClient.Data;
//using FP.ExcelReader.Abstract;
//using FP.ExcelReader.ExcelDataReader;
//using FP.ViewModel.Api;
//using FP.ViewModel.Api.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;


Debug.WriteLine($"Blazor Web Assembly Startup");
if (args.Any()) { Debug.WriteLine($"* Arguments: {string.Join(' ', args)}"); }
var baseLocation = Directory.GetCurrentDirectory();

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// configuration
var _config = builder.Configuration;
// * cannot configure options in Blazor
//builder.Services.Configure<StartupOptions>(_config.GetSection("Startup"));
builder.Logging.AddConfiguration(
    builder.Configuration.GetSection("Logging"));

var currentConfig = _config["currentConfig"];
SystemWide.StartupFolder = _config[$"{currentConfig}:dataFiles"];
string _apiBase = _config[$"{currentConfig}:TypedClient:ApiUrl"];
var _apiVer = _config[$"{currentConfig}:TypedClient:ApiVersion"];


var settings = _config.GetSection(StartupOptions.Startup);
// Needs Ms.Extensions.Options.ConfigurationExtensions 
builder.Services.Configure<StartupOptions>(settings);
//builder.Services.AddTransient<IIdentifyEventFiles, IdentifyEventFiles>();


builder.Services.AddHttpClient<ITestApiService, TestApiService>(client =>
    client.BaseAddress = new Uri("https://localhost:7264/")
);


builder.Services.Configure<HttpClientSettings>(
    _config.GetSection($"{SystemWide.CurrentConfig}:TypedClient"));
builder.Services.AddSingleton<ITypedClientConfig, TypedClientConfig>();

builder.Services.AddHttpClient<ICaseDataService, CaseDataService>(client =>
   {
       client.BaseAddress = new Uri(_apiBase);
   })
    .ConfigureHttpClient(ConfigureHttpClient);
//    .SetHandlerLifetime(TimeSpan.FromMinutes(10));
//.ConfigurePrimaryHttpMessageHandler(ConfigureHttpClientHandler());



builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });



await builder.Build().RunAsync();


static void ConfigureHttpClient(HttpClient httpClient)
{
    var userName = @"FOOTPRINT\Tom.Brown";
    // this simply passes the current windows user name to the API on each request
    var basicCreds = Convert.ToBase64String(Encoding.ASCII.GetBytes(userName + ":doesn't matter"));
    httpClient.Timeout = TimeSpan.FromSeconds(120);
    httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + basicCreds);
    httpClient.DefaultRequestHeaders.Add("X-Version", "2.0");
}


//static void ConfigureHttpClientFromSettings(IServiceProvider serviceProvider, HttpClient httpClient)
//{
//    var clientConfig = serviceProvider.GetRequiredService<ITypedClientConfig>();
//    httpClient.BaseAddress = clientConfig.ApiUrl;
//    httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.HttpTimeout);
//    var userName = WindowsIdentity.GetCurrent().Name;
//    // this simply passes the current windows user name to the API on each request
//    var basicCreds = Convert.ToBase64String(Encoding.ASCII.GetBytes(userName + ":doesn't matter"));
//    httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + basicCreds);
//    httpClient.DefaultRequestHeaders.Add("X-Version", clientConfig.ApiVersion);
//}
//static Func<IServiceProvider, HttpMessageHandler> ConfigureHttpClientHandler()
//{
//    return mh =>
//        new HttpClientHandler
//        {
//            //AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
//            //UseDefaultCredentials = true,
//        };
//}
