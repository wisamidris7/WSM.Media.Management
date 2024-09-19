using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WSM.Media.Management.Media;

namespace WSM.Media.Management;
public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseMediaFile(this IApplicationBuilder app)
    {
        return app.UseMiddleware<MediaMiddleware>();
    }
}
