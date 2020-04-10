using System;
using System.Data.HashFunction.xxHash;
using System.IO;
using System.Threading.Tasks;

namespace CardCatalog.Core
{
    public class FileProcessing
    {
        CardCatalogContext _db;
        public static readonly IxxHash _xxHash = xxHashFactory.Instance.Create();

        public FileProcessing()
        {
            _db = new CardCatalogContext();
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
            using (_db)
            {
                _db.Listings.Add(new Listing
                {
                    Checksum = checksum,
                    Created = DateTime.UtcNow,
                    FileName = fileName,
                    FilePath = filePath,
                    FileSize = fileSize,
                });

                var count = await _db.SaveChangesAsync();
                return count < 1 ? false : true;
            }
        }

        public async Task<bool> CreateTag(string tag)
        {
            using (_db)
            {
                // TODO 20-04-10 First see if tag already exists
                // and if so don't try to create a new one

                _db.Tags.Add(new Tag
                {
                    TagTitle = tag
                });

                var count = await _db.SaveChangesAsync();
                return count < 1 ? false : true;
            }
        }
    }
}