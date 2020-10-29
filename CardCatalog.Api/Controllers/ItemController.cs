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
using CardCatalog.Core.ApiModels;

using Newtonsoft.Json;

namespace CardCatalog.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemController : ControllerBase
    {
        private readonly AppSettings _appSettings;
        private readonly CardCatalogContext _db;

        public ItemController(IOptions<AppSettings> appSettings, CardCatalogContext db)
        {
            _appSettings = appSettings.Value;
            _db = db;
        }

        [AllowAnonymous]
        [HttpPost("create")]
        public async Task<IActionResult> CreateItem([FromBody] ApiNewItem body)
        {
            if (body == null || body.Description == null || body.Description == string.Empty)
            {
                return BadRequest("Item description null or empty");
            }

            var y = new ItemProcessing(_db);
            var result = await y.CreateItem(body.ContainerId, body.Description);

            return Ok(JsonConvert.SerializeObject(new ApiResponseBase { Message = "Item creation result", Success = result}));
        }

        [AllowAnonymous]
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteItem([FromBody] ApiDeleteItem body)
        {
            if (body == null || body.ItemId == null)
            {
                return BadRequest("Item id null or empty");
            }

            var y = new ItemProcessing(_db);
            var result = await y.DeleteItemById(body.ItemId);

            return Ok(JsonConvert.SerializeObject(new ApiResponseBase { Message = "Item deletion result", Success = result}));
        }

        [AllowAnonymous]
        [HttpGet("get")]
        public async Task<IActionResult> Get()
        {
            var y = new ItemProcessing(_db);
            var result = await y.GetItems();
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("get-single")]
        public async Task<IActionResult> Get(string id)
        {
            var y = new ItemProcessing(_db);
            var result = await y.GetItem(id);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("edit")]
        public async Task<IActionResult> EditItem([FromBody] ApiEditItem item)
        {
            var y = new ItemProcessing(_db);
            var result = await y.EditItem(item);
            return Ok(result);
        }
    }
}