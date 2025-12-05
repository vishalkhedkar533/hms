using AutoMapper;
using HMS.Data;
using HMS.Security;
using HMS.Services;
using Microsoft.AspNetCore.Mvc;
using Models.DB;
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
                return BadRequest("File not selected or empty.");

            if (model.UserId <= 0)
                return BadRequest("Invalid User ID.");

            try
            {
                int organisationId = Convert.ToInt32(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "OrganisationId")?.Value);
                // 1. Create uploads/userid folder
                //var userFolderPath = Path.Combine(_environment.WebRootPath, "uploads", model.UserId.ToString());
                var root = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                var userFolderPath = Path.Combine(root, model.UserId.ToString());

                // Ensure the directory exists
                if (!Directory.Exists(userFolderPath))
                {
                    Directory.CreateDirectory(userFolderPath);
                }

                // 2. Prepare file info
                var originalFileName = model.File.FileName;
                var fileExtension = Path.GetExtension(model.File.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(userFolderPath, uniqueFileName);

                // 3. Save file physically
                using (var stream = new FileStream(filePath, FileMode.Create))
                    await model.File.CopyToAsync(stream);

                var fileInfo = new FileInfo(filePath);

                // 4. Prepare entity for DB insert (FileProcessingTasks table)
                var fileTask = new FileProcessingTask
                {
                    FilePath = filePath,
                    Status = "Pending",
                    CreatedBy = model.UserId.ToString(),
                    FileName = uniqueFileName,
                    FileExtension = fileExtension,
                    FileSize = fileInfo.Length,
                    IsReadOnly = fileInfo.IsReadOnly,
                    FileCreationTime = fileInfo.CreationTime.ToUniversalTime(),
                    FileLastWriteTime = fileInfo.LastWriteTime.ToUniversalTime(),
                    TotalRows = null,
                    RowsProcessed = null,
                    RowsRejected = null,
                    CreatedAt = DateTime.UtcNow,
                    StartedAt = null,
                    CompletedAt = null,
                    ErrorMessage = null,
                    OrgId = organisationId
                };

                // 5. Save into DB using EF Core
                _context.FileProcessingTasks.Add(fileTask);
                await _context.SaveChangesAsync();

                // 6. Success response
                return Ok(new
                {
                    Message = "File uploaded and record created.",
                    FileTaskId = fileTask.Id,
                    FileName = uniqueFileName,
                    FilePath = filePath
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during file upload.");
                return StatusCode(500, "Internal server error during file upload.");
            }
        }
    }
}
