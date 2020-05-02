﻿using System;
using System.Collections.Generic;
using System.Data.HashFunction.xxHash;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CardCatalog.Core
{
    public class FileProcessing
    {
        CardCatalogContext _db;
        public static readonly IxxHash _xxHash = xxHashFactory.Instance.Create();

        public FileProcessing(CardCatalogContext context)
        {
            _db = context;
        }

        public async Task ScanFiles(string rootFilePath)
        {
            foreach (string file in GetFiles(rootFilePath))
            {
                Console.WriteLine(file + " InDB: " + FilePathInDatabase(file));

                // check if file is already in database
                if (FilePathInDatabase(file) == false)
                {
                    var info = new FileInfo(file);
                    var fileExists = info.Exists;

                    if (fileExists)
                    {
                        var hashResult = HashFile(file);
                        await CreateListing(checksum: hashResult.hash, fileName: info.Name, filePath: info.FullName, fileSize: info.Length);
                    }
                }
                else
                {
                    // do nothing
                    Console.WriteLine("File already in database: " + file);
                }
            }
        }

        public async Task DeleteOrphans(bool deleteListingOnOrphanFound)
        {
            var listings = _db.Listings;

            foreach (var listing in listings)
            {
                string filePath = listing.FilePath;

                var file = new FileInfo(filePath);

                if (file.Exists == false)
                {
                    Console.WriteLine("Orphan found: " + filePath);

                    if (deleteListingOnOrphanFound == true)
                    {
                        Console.WriteLine("Deleting orphan");
                        await DeleteListing(listing);
                    }
                }
                else
                {
                    Console.WriteLine("Not Orphan found: " + filePath);
                }
            }
        }

        public bool FilePathInDatabase(string filePath)
        {
            var check = _db.Listings.FirstOrDefault(x => x.FilePath == filePath);
            return check == null ? false : true;
        }

        public (bool present, Listing listing) ListingInDatabase(string listingId)
        {
            var check = _db.Listings.FirstOrDefault(x => x.Id == Guid.Parse(listingId));
            return check == null ? (false, null) : (true, check);
        }

        public IEnumerable<string> GetFiles(string path)
        {
            // via https://stackoverflow.com/a/929418
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(path);
            while (queue.Count > 0)
            {
                path = queue.Dequeue();
                try
                {
                    foreach (string subDir in Directory.GetDirectories(path))
                    {
                        queue.Enqueue(subDir);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                string[] files = null;
                try
                {
                    files = Directory.GetFiles(path);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                if (files != null)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        yield return files[i];
                    }
                }
            }
        }

        public (bool success, string hash) HashFile(string filePath)
        {
            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    var res = _xxHash.ComputeHash(sr.BaseStream);
                    return (true, res.AsBase64String());
                }
            }
            catch (IOException e)
            {
                return (false, string.Empty);
            }
        }

        public async Task<bool> CreateListing(string checksum, string fileName, string filePath, long fileSize)
        {
            _db.Listings.Add(new Listing
            {
                Id = Guid.NewGuid(),
                Checksum = checksum,
                Created = DateTime.UtcNow,
                FileName = fileName,
                FilePath = filePath,
                FileSize = fileSize,
            });

            var count = await _db.SaveChangesAsync();
            return count < 1 ? false : true;
        }

        public async Task DeleteListing(Listing listing)
        {
            var appliedTags = _db.ListingTags.Where(x => x.ListingRefId.Id == listing.Id);
            foreach (var x in appliedTags)
            {
                _db.ListingTags.Remove(x);
            }

            _db.Listings.Remove(listing);
            var res = await _db.SaveChangesAsync();
        }

        public async Task<(bool success, Tag tagInDatabase)> CreateTag(string tag)
        {
            var existsCheck = _db.Tags.FirstOrDefault(x => x.TagTitle.ToUpper() == tag.ToUpper());

            if (existsCheck == null)
            {
                Tag newTag = new Tag
                {
                    Id = Guid.NewGuid(),
                    TagTitle = tag
                };
                _db.Tags.Add(newTag);
                var count = await _db.SaveChangesAsync();
                return count < 1 ? (false, null) : (true, newTag);
            }
            else
            {
                return (true, existsCheck);
            }
        }

        public async Task<bool> LinkTagToListing(string listingId, string tag)
        {
            var listingIdInDatabase = ListingInDatabase(listingId);

            if (listingIdInDatabase.present == true)
            {
                var tagResult = await CreateTag(tag);

                if (tagResult.success == true)
                {
                    _db.ListingTags.Add(new ListingTag { Id = Guid.NewGuid(), ListingRefId = listingIdInDatabase.listing, TagRefId = tagResult.tagInDatabase});
                    var count = await _db.SaveChangesAsync();
                    return count < 1 ? false : true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}