using AutoMapper;
using Azure;
using HMS.Data;
using HMS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DB;
using Models.DTO;
using Models.HMSConsts;
using System.Collections.Generic;
using System.Threading.Channels;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MastersController : ControllerBase
    {
        private readonly HMSContext _context;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        public MastersController(HMSContext context, IConfiguration config, IMapper mapper)
        {
            _context = context;
            _config = config;
            _mapper = mapper;
        }

        [HttpPost("ChannelMaster")]
        //[MenuAuthorize(1001)]
        public async Task<ActionResult<ChannelMaster>> ChannelMaster([FromBody] int userid)
        {
            // var channelMaster = await _context.ChannelMaster.ToList<ChannelMaster>;
            var channelMaster = await _context.ChannelMaster.ToListAsync();

            if (channelMaster == null)
                return NotFound();

            return Ok(channelMaster);
        }

        [HttpPost("SubchannelMaster")]
        //[MenuAuthorize(1001)]
        public async Task<ActionResult<SubchannelMaster>> SubchannelMaster([FromBody] string ChannelCode)
        {
            var subchannelMaster = await _context.SubchannelMaster.Where(u => u.ChannelCode == ChannelCode).ToListAsync();
            if (subchannelMaster == null)
                return NotFound();

            return Ok(subchannelMaster);
        }
    }
}
