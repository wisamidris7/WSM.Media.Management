using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WSM.Media.Management.Models;

namespace WSM.Media.Management.Db;
public interface IMediaDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken=default);
    DbSet<MediaFile> MediaFiles { get; }
}
