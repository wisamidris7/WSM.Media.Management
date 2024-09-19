## WSM Media Management Library

A simple library to manage images or media files in ASP.NET Core with EF Core integration.

## Features

- Upload and store media files using EF Core.
- Generate URLs for uploaded media files.
- Easy integration with ASP.NET Core through dependency injection.

## Installation

1. Install the necessary NuGet packages:

```bash
dotnet add package WSM.Media.Management
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

2. Add the necessary dependencies to your project:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WSM.Media.Management;
using WSM.Media.Management.Media;
using WSM.Media.Management.Models;
using WSMMediaSample1;
```

## Setup

### 1. Create `AppDbContext`

Your `AppDbContext` must inherit from `IMediaDbContext` and include the `MediaFiles` property:

```csharp
using Microsoft.EntityFrameworkCore;
using WSM.Media.Management.Db;
using WSM.Media.Management.Models;

public class AppDbContext : DbContext, IMediaDbContext
{
    public AppDbContext(DbContextOptions options) : base(options) { }

    public DbSet<MediaFile> MediaFiles { get; set; }
}
```

### 2. Register Services in `Program.cs`

To enable media management, add the `AddMediaFile` and `UseMediaFile` services in the `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase(nameof(AppDbContext)));

// Register MediaFile management
builder.Services.AddMediaFile<AppDbContext>();

var app = builder.Build();

// Enable MediaFile middleware
app.UseMediaFile();
app.UseHttpsRedirection();
```

### 3. Upload and Manage Media Files

To upload media files, use the `FileManager` class in your API. Here’s an example of how to add media in a controller or endpoint:

```csharp
var mediaFileId = await fileManager.UploadFileAsync<WeatherForecast>(new()
{
   Bytes = Convert.FromBase64String(fileRequest.Base64),
   Extension = fileRequest.Extension,
});

var mediaFileUrl = await fileManager.GetUrlAsync(mediaFileId);
return new
{
   ResponseUrl = mediaFileUrl,
   MediaFileId = mediaFileId,
};
```

### 4. Example File Request Model

Create a simple request model for handling incoming files:

```csharp
public class ExampleFileRequest
{
    public string Extension { get; set; }
    public string Base64 { get; set; }
}
```

This setup allows you to upload media files, generate URLs, and store them using EF Core.
