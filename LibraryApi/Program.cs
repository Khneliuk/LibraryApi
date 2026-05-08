using LibraryApi.Services;

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
app.UseAuthorization();
app.MapControllers();

app.Run();