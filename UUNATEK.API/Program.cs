using UUNATEK.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<IUunaTekPlotter, UunaTekPlotter>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddHttpClient("IAuto", client =>
{
    client.BaseAddress = new Uri($"{builder.Configuration["IAuto:BaseUrl"]}");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
