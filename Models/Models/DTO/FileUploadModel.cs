using Microsoft.AspNetCore.Http;

namespace Models.DTO
{
    public class FileUploadModel
    {
        // IFormFile represents the file itself.
        public IFormFile File { get; set; }

        // Additional data submitted with the file.
        public int UserId { get; set; }
    }
}