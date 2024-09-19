using Microsoft.EntityFrameworkCore;
using WSM.Media.Management.Db;
using WSM.Media.Management.Models;

namespace WSMMediaSample1;

public class AppDbContext : DbContext, IMediaDbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<MediaFile> MediaFiles { get; set; }
}
