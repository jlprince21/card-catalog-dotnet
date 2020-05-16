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
    public class TagsController : ControllerBase
    {
        private readonly AppSettings _appSettings;
        private readonly CardCatalogContext _db;

        public TagsController(IOptions<AppSettings> appSettings, CardCatalogContext db)
        {
            _appSettings = appSettings.Value;
            _db = db;
        }

        [AllowAnonymous]
        [HttpPost("create")]
        public async Task<IActionResult> CreateTag([FromBody]ApiNewTag body)
        {
            if (body == null || body.Title == null || body.Title == string.Empty)
            {
                return BadRequest("Tag title null or empty");
            }

            var y = new FileProcessing(_db);
            var result = await y.CreateTag(body.Title);

            return Ok("Tag create result: " + result.success);
        }

        [AllowAnonymous]
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteTag([FromBody]ApiNewTag body)
        {
            if (body == null || body.Title == null || body.Title == string.Empty)
            {
                return BadRequest("Tag title null or empty");
            }

            var y = new FileProcessing(_db);
            var result = await y.DeleteTag(body.Title);

            return Ok("Tag delete result: " + result);
        }

    }
}