using Blazored.LocalStorage;
using CompanyCommander;
using CompanyCommander.DB;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddBlazoredLocalStorageAsSingleton();

builder.Services.AddSingleton<AppDbContext>();
builder.Services.AddSingleton<GameService>();
builder.Services.AddBlazorBootstrap(); // Registriert den ModalService

await builder.Build().RunAsync();
