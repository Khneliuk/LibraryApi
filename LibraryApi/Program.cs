using LibraryApi.Services;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri("http://localhost:5033/");
});

builder.Services.AddScoped<GoogleBooksService>();
builder.Services.AddScoped<DatabaseService>();

builder.Services.AddSingleton<ITelegramBotClient>(_ =>
    new TelegramBotClient("8456396061:AAHTSSB9RJew8xRdH1KCy6mxvF33cpu3rKQ"));

builder.Services.AddHostedService<TelegramBotService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger/index.html");
    return Task.CompletedTask;
});

app.UseHttpsRedirection();
app.MapControllers();

app.Run();