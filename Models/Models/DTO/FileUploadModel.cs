using Microsoft.AspNetCore.Http;

namespace Models.DTO
{
    public class FileUploadModel
    {
        // IFormFile represents the file itself.
        public IFormFile File { get; set; }

        // Additional data submitted with the file.
        public int UserId { get; set; }

        // Selected file type (e.g., manager_update, policy, premium_collected, agent_create).
        public string FileType { get; set; }
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