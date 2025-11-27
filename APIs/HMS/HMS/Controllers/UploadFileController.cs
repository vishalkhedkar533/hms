using AutoMapper;
using HMS.Data;
using HMS.Security;
using HMS.Services;
using Microsoft.AspNetCore.Mvc;
using Models.DTO;

namespace HMS.Controllers
{
    public class UploadFileController : Controller
    {
        private readonly HMSContext _context;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly DatabaseService _db;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<UploadFileController> _logger;
        public UploadFileController(HMSContext context, 
            IConfiguration config,IMapper mapper, 
            DatabaseService db,IWebHostEnvironment environment,
            ILogger<UploadFileController> logger)
        {
            _context = context;
            _config = config;
            _mapper = mapper;
            _db = db;
            _environment = environment;
            _logger = logger;
        }

        [HttpPost("file")]
        [DisableRequestSizeLimit]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> UploadFile([FromForm] FileUploadModel model)
        {
            if (model.File == null || model.File.Length == 0)
            {
                return BadRequest("File not selected or is empty.");
            }

            if (model.UserId <= 0)
            {
                return BadRequest("Invalid User ID provided.");
            }

            try
            {
                // 1. Define storage path (e.g., a specific folder for the user)
                var userFolderPath = Path.Combine(_environment.WebRootPath, "uploads", model.UserId.ToString());

                // Ensure the directory exists
                if (!Directory.Exists(userFolderPath))
                {
                    Directory.CreateDirectory(userFolderPath);
                }

                // 2. Create a unique file name
                // Use Guid to ensure uniqueness and combine with the original extension
                var fileExtension = Path.GetExtension(model.File.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(userFolderPath, uniqueFileName);

                // 3. Save the file to the physical location
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.File.CopyToAsync(stream);
                }

                _logger.LogInformation($"File '{uniqueFileName}' uploaded successfully for User ID: {model.UserId}.");

                // 4. Return success response
                return Ok(new { Message = "File uploaded successfully", FileName = uniqueFileName, UserId = model.UserId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during file upload for User ID: {UserId}", model.UserId);
                return StatusCode(500, "Internal server error during file upload.");
            }
        }
    }
}
