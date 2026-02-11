using Microsoft.AspNetCore.Http;

namespace Models.DTO
{
    public class FileUploadModel
    {
        public required IFormFile File { get; set; }

        public string? FileType { get; set; }
    }

    public class BatchListDto
    {
        public int BatchId { get; set; }
        public string? FileName { get; set; }
        public string? UploadedBy { get; set; }
        public DateTime UploadedOn { get; set; }
        public int Total { get; set; }
        public int Success { get; set; }
        public int Failed { get; set; }
        public string? Status { get; set; }
    }
}