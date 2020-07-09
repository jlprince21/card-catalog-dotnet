using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardCatalog.Api.Helpers;
using CardCatalog.Api.Models;
using CardCatalog.Core;

namespace CardCatalog.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContainerController : ControllerBase
    {
        private readonly AppSettings _appSettings;
        private readonly CardCatalogContext _db;

        public ContainerController(IOptions<AppSettings> appSettings, CardCatalogContext db)
        {
            _appSettings = appSettings.Value;
            _db = db;
        }

        [AllowAnonymous]
        [HttpPost("create")]
        public async Task<IActionResult> CreateContainer([FromBody]ApiNewContainer body)
        {
            if (body == null || body.Description == null || body.Description == string.Empty)
            {
                return BadRequest("Container description null or empty");
            }

            var y = new ItemProcessing(_db);
            var result = await y.CreateContainer(body.Description);

            return Ok("Container create result: " + result); // TODO 2020-05-28 return GUID of container in tuple?
        }

        [AllowAnonymous]
        [HttpGet("get")]
        public async Task<IActionResult> Get()
        {
            var y = new ItemProcessing(_db);
            var result = await y.GetContainers();
            return Ok(result);
        }

        // [AllowAnonymous]
        // [HttpPost("delete")]
        // public async Task<IActionResult> DeleteTag([FromBody]ApiNewTag body)
        // {
        //     if (body == null || body.Title == null || body.Title == string.Empty)
        //     {
        //         return BadRequest("Tag title null or empty");
        //     }

        //     var y = new FileProcessing(_db);
        //     var result = await y.DeleteTag(body.Title);

        //     return Ok("Tag delete result: " + result);
        // }

        // TODO 2020-05-17 Applying tags will need to consider both nullable foreign key
        // columns. Be sure to have enough data in model to know which to apply to!
    }
}