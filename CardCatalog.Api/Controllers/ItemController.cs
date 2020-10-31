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

            var response = new Response<string>(){Message = "Item creation result", Success = result};
            return Ok(JsonConvert.SerializeObject(response));
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

            var response = new Response<string>(){Message = "Item deletion result", Success = result};
            return Ok(JsonConvert.SerializeObject(response));
        }

        [AllowAnonymous]
        [HttpGet("get")]
        public async Task<IActionResult> Get()
        {
            var result = new PagedResponse<Item>(items: new List<Item>());
            var y = new ItemProcessing(_db);
            result.Items = await y.GetItems();
            result.TotalRecords = result.Items.Any() ? result.Items.Count : 0;
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("get-single")]
        public async Task<IActionResult> Get(string id)
        {
            var result = new PagedResponse<SingleItem>(items: new List<SingleItem>());
            var y = new ItemProcessing(_db);
            var someItem = await y.GetItem(id);
            result.Items.Add(someItem);
            return Ok(JsonConvert.SerializeObject(result));
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