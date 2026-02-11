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
using System.Security.Claims;

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

            if (string.IsNullOrWhiteSpace(model.FileType))
                return BadRequest("File type is required.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return NotFound("UserId claim not found or invalid.");
            }

            var fileType = model.FileType.Trim().ToLowerInvariant();
            HmsResponse hmsResponse = new HmsResponse();

            try
            {
                int organisationId = Convert.ToInt32(Convert.ToInt64(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0"));
                var root = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                var userFolderPath = Path.Combine(root, userId.ToString());

                if (!Directory.Exists(userFolderPath))
                {
                    Directory.CreateDirectory(userFolderPath);
                }

                var fileExtension = Path.GetExtension(model.File.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(userFolderPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await model.File.CopyToAsync(stream);

                var fileInfo = new FileInfo(filePath);

                var fileTask = new FileProcessingTask
                {
                    FilePath = filePath,
                    Status = "Pending",
                    CreatedBy = userId.ToString(),
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

                _context.FileProcessingTasks.Add(fileTask);
                await _context.SaveChangesAsync();

                hmsResponse.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                hmsResponse.responseHeader.ErrorMessage = "SUCCESS";
                hmsResponse.responseBody.fileUpload = new FileUploadResponse
                {
                    FileTaskId = fileTask.Id,
                    FileName = uniqueFileName,
                    FilePath = filePath,
                    FileType = fileType
                };

                return Ok(hmsResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during file upload.");
                return StatusCode(500, "Internal server error");
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

        [HttpPost("download-report/{id}/{reportType}")]
        public async Task<IActionResult> DownloadReport([FromRoute] int id, [FromRoute] string reportType)
        {
            HmsResponse hmsResponse = new HmsResponse();
            try
            {
                var task = await _context.FileProcessingTasks.FindAsync(id);
                if (task == null)
                {
                    hmsResponse.responseHeader.ErrorCode = CommonConstants.FAILED;
                    hmsResponse.responseHeader.ErrorMessage = "Task not found.";
                    return NotFound(hmsResponse);
                }

                string? base64Content = null;
                string suffix = "";

                if (reportType.ToLower() == "success")
                {
                    base64Content = task.SuccessData;
                    suffix = "_successRows";
                }
                else
                {
                    base64Content = task.ErrorMessage;
                    suffix = "_failedRows";
                }

                if (string.IsNullOrEmpty(base64Content))
                {
                    hmsResponse.responseHeader.ErrorCode = CommonConstants.FAILED;
                    hmsResponse.responseHeader.ErrorMessage = $"No {reportType} report available.";
                    return BadRequest(hmsResponse);
                }

                byte[] fileBytes = Convert.FromBase64String(base64Content);
                var fileBaseName = string.IsNullOrWhiteSpace(task.FileName) ? "report" : Path.GetFileNameWithoutExtension(task.FileName);
                var fileName = $"{fileBaseName}{suffix}.xlsx";

                hmsResponse.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                hmsResponse.responseHeader.ErrorMessage = "SUCCESS";
                hmsResponse.responseBody.fileDownload = new FileDownloadDto
                {
                    FileName = fileName,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    FileBase64 = base64Content,
                    FileSize = fileBytes.LongLength
                };

                return Ok(hmsResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading {ReportType} report for ID {Id}", reportType, id);
                hmsResponse.responseHeader.ErrorCode = CommonConstants.FAILED;
                hmsResponse.responseHeader.ErrorMessage = "Internal Server Error";
                return StatusCode(500, hmsResponse);
            }
        }
    }
}
