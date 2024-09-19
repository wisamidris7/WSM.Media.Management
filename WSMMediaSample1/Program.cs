using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WSM.Media.Management;
using WSM.Media.Management.Media;
using WSM.Media.Management.Models;
using WSMMediaSample1;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(e =>
{
    e.UseInMemoryDatabase(nameof(AppDbContext));
});
// Add this line
builder.Services.AddMediaFile<AppDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMediaFile();
app.UseHttpsRedirection();


app.MapPost("/mediaadd", async ([FromBody] ExampleFileRequest fileRequest, [FromServices] FileManager fileManager) =>
{
    var mediaFileId = await fileManager.UploadFileAsync<WeatherForecast>(new()
    {
        Bytes = Convert.FromBase64String(fileRequest.Base64),
        Extension = fileRequest.Extension,
    });
    var mediaFileUrl = fileManager.GetUrlAsync(mediaFileId);
    return new
    {
        ResponseUrl = mediaFileUrl,
        MediaFileId = mediaFileId,
    };
})
.WithName("AddMedia")
.WithOpenApi();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
public class ExampleFileRequest
{
    public string Extension { get; set; }
    public string Base64 { get; set; }
}