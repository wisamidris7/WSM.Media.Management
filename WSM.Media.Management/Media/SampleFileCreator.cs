using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WSM.Media.Management.Models;

namespace WSM.Media.Management.Media;
public class SampleFileCreator
{
    public async Task<Guid?> CreateSample<T>(FileManager fileManager, string fileName)
    {
        return await fileManager.UploadFileAsync<T>(new()
        {
            Bytes = File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "Sample", fileName)),
            Extension = Path.GetExtension(fileName),
            Type = MediaFileType.Image
        });
    }
}
