using System;
using System.Collections.Generic;
using System.Data.HashFunction.xxHash;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CardCatalog.Core.ApiModels;

namespace CardCatalog.Core
{
    public class ItemProcessing
    {
        CardCatalogContext _db;
        public static readonly IxxHash _xxHash = xxHashFactory.Instance.Create();

        public ItemProcessing(CardCatalogContext context)
        {
            _db = context;
        }

        public async Task<bool> CreateContainer(string description)
        {
            _db.Containers.Add(new Container
            {
                Id = Guid.NewGuid(),
                Description = description
            });

            var count = await _db.SaveChangesAsync();
            return count < 1 ? false : true;
        }

        public (bool present, Container container) ContainerInDatabase(string containerId)
        {
            var check = _db.Containers.FirstOrDefault(x => x.Id == Guid.Parse(containerId));
            return check == null ? (false, null) : (true, check);
        }

        public (bool present, Item item) ItemInDatabase(string itemId)
        {
            var check = _db.Items.FirstOrDefault(x => x.Id == Guid.Parse(itemId));
            return check == null ? (false, null) : (true, check);
        }

        public async Task<bool> CreateItem(string containerId, string itemDescription)
        {
            var containerIdInDatabase = ContainerInDatabase(containerId);

            if (containerIdInDatabase.present == true)
            {
                _db.Items.Add(new Item
                {
                    Id = Guid.NewGuid(),
                    ContainerRefId = containerIdInDatabase.container,
                    Description = itemDescription
                });
                var count = await _db.SaveChangesAsync();
                return count < 1 ? false : true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> DeleteItemById(string itemId)
        {
            var itemIdInDatabase = ItemInDatabase(itemId);

            if (itemIdInDatabase.present == true)
            {
                return await DeleteItem(itemIdInDatabase.item);
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> DeleteItem(Item item)
        {
            // TODO when tag support for items is implemented will need to untag here
            _db.Items.Remove(item);
            var count = await _db.SaveChangesAsync();
            return count < 1 ? false : true;
        }

        public async Task<List<Container>> GetContainers()
        {
            return _db.Containers.ToList();
        }

        public async Task<List<Item>> GetItems()
        {
            // .Include tells EF to eagerly load foreign key data
            var results = _db.Items
                            .Include(item => item.ContainerRefId)
                            .ToList();
            return results;
        }

        public async Task<SingleItem> GetItem(string id)
        {
            // TODO this should probably check if item in DB and set a bool or something nicer lol
            var res = (from i in _db.Items
                       join c in _db.Containers
                       on i.ContainerRefId.Id equals c.Id
                       where i.Id == new Guid(id)
                       select new
                       {
                           Id = i.Id,
                           ContainerId = c.Id,
                           Description = i.Description
                       }
                      ).FirstOrDefault();

            var singleItem = new SingleItem();
            singleItem.ContainerId = res.ContainerId;
            singleItem.Description = res.Description;
            singleItem.ItemId = res.Id;

            return singleItem;
        }

        public async Task<bool> EditItem(ApiEditItem item)
        {
            var entity = _db.Items.FirstOrDefault(x => x.Id == Guid.Parse(item.ItemId));

            if (entity != null)
            {
                entity.Description = item.Description;
                var count = await _db.SaveChangesAsync();
                return count == 1 ? true : false;
            }
            else
            {
                return false;
            }
        }
    }
}