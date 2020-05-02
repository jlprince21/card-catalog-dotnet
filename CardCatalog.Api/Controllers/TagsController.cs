using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardCatalog.Api.Helpers;
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
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var y = new FileProcessing(_db);
            var createTagResult = await y.CreateTag("test2");
            Console.WriteLine("createTagResult: " + createTagResult);

            return Ok("hello from tags" + createTagResult.success);
        }
    }
}