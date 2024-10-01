using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using SignalRGame.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(sp =>
{
	var navigationManager = sp.GetRequiredService<NavigationManager>();

	var hubConnection = new HubConnectionBuilder()
		.WithUrl(navigationManager.ToAbsoluteUri("https://localhost:7162/gamehub"))
		.WithAutomaticReconnect()
		.Build();

	hubConnection.StartAsync();

	return hubConnection;
});

builder.Services.AddSingleton<GameClient>();

await builder.Build().RunAsync();
