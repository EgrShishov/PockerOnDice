using SignalRGame.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
	options.AddPolicy("blazor", builder =>
	{
		builder.WithOrigins("https://localhost:7267")
			   .AllowAnyHeader()
			   .AllowAnyMethod()
			   .AllowCredentials();
	});
});

var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors("blazor");

app.MapHub<GameHub>("/gamehub");

app.Run();
