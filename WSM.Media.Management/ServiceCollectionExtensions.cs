using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WSM.Media.Management.Db;
using WSM.Media.Management.Media;

namespace WSM.Media.Management;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediaFile<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext, IMediaDbContext
    {
        services.AddScoped<MediaMiddleware>();
        services.AddScoped<FileManager>();
        services.AddScoped<SampleFileCreator>();
        services.AddScoped<IMediaDbContext>(e => e.GetRequiredService<TDbContext>());
        return services;
    }
}
