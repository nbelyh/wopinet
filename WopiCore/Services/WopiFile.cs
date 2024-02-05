using System;

namespace WopiCore.Services
{
    public class WopiFile
    {
        public string FileId { get; set; }
        public string LockValue { get; set; }
        public DateTime? LockExpires { get; set; }
        public string OwnerId { get; set; }
        public string FileName { get; set; }
        public long Size { get; set; }
        public int Version { get; set; }
        public DateTimeOffset LastModifiedTime { get; set; }
        public string LastModifiedUser { get; set; }
    }
}
