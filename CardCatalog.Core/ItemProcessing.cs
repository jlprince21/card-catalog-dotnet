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

        public async Task<InsertSuccessAndId> CreateContainer(string description)
        {
            var newContainer = new Container
            {
                Id = Guid.NewGuid(),
                Description = description
            };

            _db.Containers.Add(newContainer);
            var count = await _db.SaveChangesAsync();

            var res = new InsertSuccessAndId{id = newContainer.Id, success = count < 1 ? false : true};

            return res;
        }

        public async Task<bool> DeleteContainer(string containerId)
        {
            var containerIdInDatabase = ContainerInDatabase(containerId);

            if (containerIdInDatabase.present == true)
            {
                return await DeleteContainer(containerIdInDatabase.container);
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> DeleteContainer(Container container)
        {
            // TODO 2020-12-06 will need to mark items as outside container here in future
            _db.Containers.Remove(container);
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

        public async Task<List<SingleContainer>> GetContainers()
        {
            List<SingleContainer> results = (from i in _db.Containers
                                             select new SingleContainer
                                             {
                                                 id = i.Id,
                                                 description = i.Description,
                                             }
                                            ).OrderBy(x => x.description).ToList();

            return results;
        }

        public async Task<List<SingleItem>> GetItems()
        {
            List<SingleItem> results = (from i in _db.Items
                                        join c in _db.Containers
                                        on i.ContainerRefId.Id equals c.Id
                                        select new SingleItem
                                        {
                                            itemId = i.Id,
                                            containerId = c.Id,
                                            itemDescription = i.Description,
                                            containerDescription = c.Description
                                        }
                       ).ToList();

            return results;
        }

        public async Task<SingleItem> GetItem(Guid id)
        {
            // TODO this should probably check if item in DB and set a bool or something nicer lol
            var res = (from i in _db.Items
                       join c in _db.Containers
                       on i.ContainerRefId.Id equals c.Id
                       where i.Id == id
                       select new SingleItem
                       {
                           itemId = i.Id,
                           containerId = c.Id,
                           itemDescription = i.Description,
                           containerDescription = c.Description
                       }
                      ).FirstOrDefault();

            return res;
        }

        public async Task<bool> EditItem(ApiEditItem item)
        {
            var entity = _db.Items.FirstOrDefault(x => x.Id == Guid.Parse(item.itemId));

            if (entity != null)
            {
                entity.Description = item.description;
                var count = await _db.SaveChangesAsync();
                return count == 1 ? true : false;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> EditContainer(ApiEditContainer container)
        {
            var entity = _db.Containers.FirstOrDefault(x => x.Id == Guid.Parse(container.containerId));

            if (entity != null)
            {
                entity.Description = container.description;
                var count = await _db.SaveChangesAsync();
                return count == 1 ? true : false;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> MoveItem(ApiMoveItem item)
        {
            var entity = _db.Items.FirstOrDefault(x => x.Id == Guid.Parse(item.itemId));
            var containerEntity = _db.Containers.FirstOrDefault(x => x.Id == Guid.Parse(item.containerId));

            if (entity != null)
            {
                entity.ContainerRefId = containerEntity;
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