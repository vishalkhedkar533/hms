using CommonLibrary.Background;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Controllers
{
    [ApiController]
    [Route("api/upload")]
    public class UploadController : ControllerBase
    {
        private readonly IExcelProcessingQueue _queue;


        public UploadController(IExcelProcessingQueue queue)
        {
            _queue = queue;
        }


        [HttpPost]
        public IActionResult Upload(IFormFile file)
        {
            if (file is null || file.Length == 0)
                return BadRequest("No file uploaded");


            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Path.GetExtension(file.FileName));


            using (var stream = System.IO.File.Create(tempPath))
                file.CopyTo(stream);

            _queue.Enqueue(tempPath);
            return Ok("File queued for processing.");
        }
    }
}
