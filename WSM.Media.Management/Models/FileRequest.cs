using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WSM.Media.Management.Models;
public class FileRequest
{
    public string Extension { get; set; }
    public Guid? FileId { get; set; }
    public byte[] Bytes { get; set; } = [];
    public bool Delete { get; set; }
    public MediaFileType Type { get; set; }
    public enum RequestState
    {
        Nothing,
        Edit,
        Add,
        Delete
    }
}