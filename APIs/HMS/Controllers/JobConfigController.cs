using HMS.Data;
using HMS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DB;
using System;

namespace HMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobConfigController : ControllerBase
    {
        private readonly HMSContext _context;

        public JobConfigController(HMSContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] JobConfig jobConfig)
        {
            if (jobConfig == null)
                return BadRequest("Invalid request");

            jobConfig.CreatedAt = DateTime.UtcNow;

            _context.JobConfigs.Add(jobConfig);

            try
            {
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById),
                    new { id = jobConfig.JobConfigId }, jobConfig);
            }
            catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
            {
                return Conflict(new
                {
                    message = "An enabled job with the same job name already exists for this organisation."
                });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var job = await _context.JobConfigs.FindAsync(id);
            return job == null ? NotFound() : Ok(job);
        }
    }
}
