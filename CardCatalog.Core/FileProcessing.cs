using System;
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

        public async Task<bool> ScanFiles(string rootFilePath)
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
            Console.WriteLine("Processing complete");
            return true;
        }

        /// <summary>
        /// Finds files present in database but not on disk and removes them from database
        /// if told to do so.
        /// </summary>
        /// <param name="deleteListingOnOrphanFound">When true, deletes listing and associations if found to be orphan.</param>
        /// <returns></returns>
        public async Task DeleteOrphans(bool deleteListingOnOrphanFound)
        {
            long orphansFound = 0;
            var listings = _db.Files;

            foreach (var listing in listings)
            {
                string filePath = listing.FilePath;
                var file = new FileInfo(filePath);

                if (file.Exists == false)
                {
                    orphansFound += 1;
                    Console.WriteLine("Orphan found: " + filePath);

                    if (deleteListingOnOrphanFound == true)
                    {
                        Console.WriteLine("Deleting orphan");
                        var result = await DeleteListing(listing);
                        Console.WriteLine("Deletion result: " + result);
                    }
                }
                else
                {
                    Console.WriteLine("File found: " + filePath);
                }
            }

            Console.WriteLine("Orphans found: " + orphansFound);
        }

        public bool FilePathInDatabase(string filePath)
        {
            var check = _db.Files.FirstOrDefault(x => x.FilePath == filePath);
            return check == null ? false : true;
        }

        public (bool present, File listing) ListingInDatabase(string listingId)
        {
            var check = _db.Files.FirstOrDefault(x => x.Id == Guid.Parse(listingId));
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

        /// <summary>
        /// Computes a XxHash of a file given its path.
        /// </summary>
        /// <param name="filePath">Path to a file.</param>
        /// <returns>Tuple with a bool indicating success of hashing and a hash if successful.</returns>
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

        /// <summary>
        /// Creates a new listing.
        /// </summary>
        /// <param name="checksum">Checksum of file.</param>
        /// <param name="fileName">Name of file.</param>
        /// <param name="filePath">Path to file.</param>
        /// <param name="fileSize">Size in bytes of file.</param>
        /// <returns>Bool indicating success/failure.</returns>
        public async Task<bool> CreateListing(string checksum, string fileName, string filePath, long fileSize)
        {
            _db.Files.Add(new File
            {
                Id = Guid.NewGuid(),
                Checksum = checksum,
                FoundOn = DateTime.UtcNow,
                FileName = fileName,
                FilePath = filePath,
                FileSize = fileSize,
            });

            var count = await _db.SaveChangesAsync();
            return count >= 1 ? true : false;
        }

        /// <summary>
        /// Deletes a listing along with all associated entities (eg tags).
        /// </summary>
        /// <param name="listing">Listing to be deleted.</param>
        /// <returns>Bool indicating success/failure.</returns>
        public async Task<bool> DeleteListing(File listing)
        {
            var appliedTags = _db.AppliedTags.Where(x => x.FileRefId.Id == listing.Id);
            foreach (var x in appliedTags)
            {
                _db.AppliedTags.Remove(x);
            }

            _db.Files.Remove(listing);
            var count = await _db.SaveChangesAsync();
            return count >= 1 ? true : false;
        }

        /// <summary>
        /// Creates a new tag.
        /// </summary>
        /// <param name="tag">Text of tag to create.</param>
        /// <returns>Bool indicating success/failure.</returns>
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

        /// <summary>
        /// Deletes a tag entirely including tagged listings and the tag itself.
        /// </summary>
        /// <param name="tag">Text of tag to remove.</param>
        /// <returns>Bool indicating success/failure.</returns>
        public async Task<bool> DeleteTag(string tag)
        {
            var existsCheck = _db.Tags.FirstOrDefault(x => x.TagTitle.ToUpper() == tag.ToUpper());

            if (existsCheck == null)
            {
                return false;
            }
            else
            {
                var result = await DisassociateTagFromAllListings(existsCheck.Id);

                if (result == true)
                {
                    _db.Tags.Remove(existsCheck);
                    var count = await _db.SaveChangesAsync();
                    return count == 1 ? true : false;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Disassociates a tag from listings but doesn't delete the tag itself.
        /// </summary>
        /// <param name="tagId">Id of tag to have associations removed from.</param>
        /// <returns>Bool indicating a likely success if at least one association deleted.</returns>
        public async Task<bool> DisassociateTagFromAllListings(Guid tagId)
        {
            var appliedTags = _db.AppliedTags.Where(x => x.TagRefId.Id == tagId);
            foreach (var x in appliedTags)
            {
                _db.AppliedTags.Remove(x);
            }

            var count = await _db.SaveChangesAsync();
            return count >= 1 ? true : false;
        }

        /// <summary>
        /// Links a tag to a database, creating tag if needed.
        /// </summary>
        /// <param name="listingId">Id of listing to apply tag to.</param>
        /// <param name="tag">Tag to apply to listing.</param>
        /// <returns>Bool indicating success/failure.</returns>
        public async Task<bool> LinkTagToListing(string listingId, string tag)
        {
            var listingIdInDatabase = ListingInDatabase(listingId);

            if (listingIdInDatabase.present == true)
            {
                var tagResult = await CreateTag(tag);

                if (tagResult.success == true)
                {
                    _db.AppliedTags.Add(new AppliedTag { Id = Guid.NewGuid(), FileRefId = listingIdInDatabase.listing, TagRefId = tagResult.tagInDatabase});
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