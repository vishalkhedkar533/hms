using AutoMapper;
using CommonLibrary;
using HMS.Data;
using HMS.Security;
using HMS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DB;
using Models.DTO;
using Models.HMSConsts;

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
        private readonly IAuthClaimService _authClaimService;
        public UploadFileController(HMSContext context, 
            IConfiguration config,IMapper mapper, 
            DatabaseService db,IWebHostEnvironment environment,
            ILogger<UploadFileController> logger,
            IAuthClaimService authClaimService)
        {
            _context = context;
            _config = config;
            _mapper = mapper;
            _db = db;
            _environment = environment;
            _logger = logger;
            _authClaimService = authClaimService;
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

            if (string.IsNullOrWhiteSpace(model.FileType))
                return BadRequest("File type is required.");

            var fileType = model.FileType.Trim().ToLowerInvariant();

            try
            {
                int organisationId = Convert.ToInt32(Convert.ToInt64(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0"));
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
                    FileType = fileType,
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
                    FilePath = filePath,
                    FileType = fileType,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during file upload.");
                return StatusCode(500, "Internal server error during file upload.");
            }
        }

        [HttpPost("uploaded-file-list")]
        public async Task<IActionResult> GetUploadedFiles([FromQuery] string month, [FromQuery] string status)
        {
            HmsResponse hmsResponse = new HmsResponse();
            try
            {
                var query = _context.FileProcessingTasks.AsQueryable();

                if (!string.IsNullOrEmpty(status) && status != "All")
                    query = query.Where(x => x.Status == status);

                //// 1. Status Filter
                //if (!string.IsNullOrEmpty(status) && status != "All")
                //{
                //    query = query.Where(x => x.Status == status);
                //}

                //// 2. Month Filter
                //if (!string.IsNullOrEmpty(month))
                //{
                //    if (DateTime.TryParse(month, out DateTime filterDate))
                //    {
                //        // Filter by the Month and Year of CreatedAt
                //        query = query.Where(x => x.CreatedAt.Month == filterDate.Month &&
                //                                 x.CreatedAt.Year == filterDate.Year);
                //    }
                //}
                var data = await query.OrderByDescending(x => x.CreatedAt)
                    .Select(x => new BatchListDto
                    {
                        BatchId = x.Id,
                        FileName = x.FileName,
                        UploadedBy = x.CreatedBy,
                        UploadedOn = x.CreatedAt,
                        Total = x.TotalRows ?? 0,
                        Success = x.RowsProcessed ?? 0,
                        Failed = x.RowsRejected ?? 0,
                        Status = x.Status
                    }).ToListAsync();

                if (data.Any())
                {
                    hmsResponse.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                    hmsResponse.responseHeader.ErrorMessage = "SUCCESS";
                    hmsResponse.responseBody.batches = data;
                    return Ok(hmsResponse);
                }

                hmsResponse.responseHeader.ErrorCode = CommonConstants.FAILED;
                hmsResponse.responseHeader.ErrorMessage = "No batches found.";
                return NotFound(hmsResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching uploaded batches");
                hmsResponse.responseHeader.ErrorCode = CommonConstants.FAILED;
                hmsResponse.responseHeader.ErrorMessage = "Internal Server Error";
                return StatusCode(500, hmsResponse);
            }
        }

        [HttpPost("failure-report/{id}")]
        public async Task<IActionResult> DownloadLog([FromRoute]int id)
        {
            HmsResponse hmsResponse = new HmsResponse();
            try
            {
                var task = await _context.FileProcessingTasks.FindAsync(id);
                if (task == null)
                {
                    hmsResponse.responseHeader.ErrorCode = CommonConstants.FAILED;
                    hmsResponse.responseHeader.ErrorMessage = "Batch not found.";
                    return NotFound(hmsResponse);
                }

                if (string.IsNullOrEmpty(task.ErrorMessage))
                {
                    hmsResponse.responseHeader.ErrorCode = CommonConstants.FAILED;
                    hmsResponse.responseHeader.ErrorMessage = "No error log available.";
                    return BadRequest(hmsResponse);
                }

                var fileBytes = Convert.FromBase64String(task.ErrorMessage);
                var fileBaseName = string.IsNullOrWhiteSpace(task.FileName) ? "failedRows" : task.FileName.Trim();
                var fileName = $"{fileBaseName}_failedRows.xlsx";

                hmsResponse.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                hmsResponse.responseHeader.ErrorMessage = "SUCCESS";
                hmsResponse.responseBody.fileDownload = new FileDownloadDto
                {
                    FileName = fileName,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    FileBase64 = Convert.ToBase64String(fileBytes),
                    FileSize = fileBytes.LongLength
                };

                return Ok(hmsResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading batch log for {BatchId}", id);
                hmsResponse.responseHeader.ErrorCode = CommonConstants.FAILED;
                hmsResponse.responseHeader.ErrorMessage = "Internal Server Error";
                return StatusCode(500, hmsResponse);
            }
        }
    }
}
