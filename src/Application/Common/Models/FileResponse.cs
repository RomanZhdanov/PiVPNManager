namespace PiVPNManager.Application.Common.Models
{
    public sealed class FileResponse
    {
        public string FileName { get; set; }

        public string ContentType { get; set; }

        public byte[] FileContents { get; set; }
    }
}
