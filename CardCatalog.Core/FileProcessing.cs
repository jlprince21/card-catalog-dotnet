// System namespaces

// Third-party namespaces (NuGet packages)
using System.Data.HashFunction.xxHash;

// Project-specific namespaces
using CardCatalog.Core.Models;
using CardCatalog.Core.Services;

namespace CardCatalog.Core;

public class FileProcessing
{
    CardCatalogContext _db;
    ITimeService _timeService;
    public static readonly IxxHash _xxHash = xxHashFactory.Instance.Create();

    public FileProcessing(CardCatalogContext context, ITimeService timeService)
    {
        _db = context;
        _timeService = timeService;
    }

    public async Task<bool> ScanFiles(string rootFilePath)
    {
        var jobId = Guid.NewGuid();
        _db.Jobs.Add(new Job { Id = jobId, Data = "Test", RunStartTime = _timeService.Now, Type = Common.JobType.Scan});
        _db.SaveChanges();
        var currentJob = _db.Jobs.Where(job => job.Id == jobId).FirstOrDefault();

        long counter = 0;
        var inDb = false;

        foreach (string file in GetFiles(rootFilePath))
        {
            counter++;
            inDb = FilePathInDatabase(file);
            Console.WriteLine($"{counter} processed. Current: {file} InDB: {inDb}");

            // check if file is already in database
            if (inDb == false)
            {
                var info = new FileInfo(file);
                var fileExists = info.Exists;

                if (fileExists)
                {
                    var hashResult = HashFile(file);
                    await CreateFile(currentJob, checksum: hashResult.hash, fileName: info.Name, filePath: info.FullName, fileSize: info.Length);
                }
            }
            else
            {
                // do nothing
                Console.WriteLine("File already in database: " + file);
            }
        }

        currentJob.RunEndTime = _timeService.Now;
        _db.SaveChanges();

        Console.WriteLine("Processing complete");
        return true;
    }

    /// <summary>
    /// Finds files present in database but not on disk and removes them from database
    /// if told to do so.
    /// </summary>
    /// <param name="deleteFileOnMissingFound">When true, deletes file and associations if found to be missing.</param>
    /// <returns></returns>
    public async Task DeleteMissing(bool deleteFileOnMissingFound)
    {
        // TODO 2024-07-03 File extension casing will not detect a difference and will count
        // the file as existing, though it technically doesn't. Eg, ".JPG" renamed to ".jpg"
        // will currently *not* show up as missing.
        var jobId = Guid.NewGuid();
        _db.Jobs.Add(new Job { Id = jobId, Data = "Test", RunStartTime = _timeService.Now, Type = Common.JobType.Missing});
        _db.SaveChanges();
        var currentJob = _db.Jobs.Where(job => job.Id == jobId).FirstOrDefault();

        long missingFound = 0;
        var files = _db.Files.ToList();

        long counter = 0;

        foreach (var file in files)
        {
            counter++;
            string filePath = file.FilePath;
            var fileInfo = new FileInfo(filePath);

            if (fileInfo.Exists == false)
            {
                missingFound += 1;
                Console.WriteLine("Missing found: " + filePath);

                if (deleteFileOnMissingFound == true)
                {
                    Console.WriteLine("Deleting missing");
                    var result = await DeleteListing(file);
                    Console.WriteLine("Deletion result: " + result);
                }
            }

            if (counter % 100 == 0)
            {
                Console.WriteLine($"{counter} processed. {missingFound} missing. Last checked: " + filePath);
            }
        }

        currentJob.RunEndTime = _timeService.Now;
        _db.SaveChanges();
        Console.WriteLine("Missing found: " + missingFound);
    }

    public bool FilePathInDatabase(string filePath)
    {
        var check = _db.Files.FirstOrDefault(x => x.FilePath == filePath);
        return check == null ? false : true;
    }

    public (bool present, Models.File? file) ListingInDatabase(string fileId)
    {
        var check = _db.Files.FirstOrDefault(x => x.Id == Guid.Parse(fileId));
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

            string[] files = new string[0];

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
        catch (IOException)
        {
            return (false, string.Empty);
        }
    }

    /// <summary>
    /// Creates a new file.
    /// </summary>
    /// <param name="checksum">Checksum of file.</param>
    /// <param name="fileName">Name of file.</param>
    /// <param name="filePath">Path to file.</param>
    /// <param name="fileSize">Size in bytes of file.</param>
    /// <returns>Bool indicating success/failure.</returns>
    public async Task<bool> CreateFile(Job currentJob, string checksum, string fileName, string filePath, long fileSize)
    {
        _db.Files.Add(new Models.File
        {
            Id = Guid.NewGuid(),
            Job = currentJob,
            Checksum = checksum,
            FoundOn = DateTime.UtcNow,
            FileName = fileName,
            FilePath = filePath,
            FileSize = fileSize,
        });

        var count = _db.SaveChanges(); // TODO 2021-06-07 No apparent reason why SaveChangesAsync won't work here
        return count >= 1 ? true : false;
    }

    /// <summary>
    /// Deletes a file along with all associated entities (eg tags).
    /// </summary>
    /// <param name="file">Listing to be deleted.</param>
    /// <returns>Bool indicating success/failure.</returns>
    public async Task<bool> DeleteListing(Models.File file)
    {
        var appliedTags = _db.AppliedTags.Where(x => x.FileRefId.Id == file.Id).ToList();
        foreach (var x in appliedTags)
        {
            _db.AppliedTags.Remove(x);
        }

        _db.Files.Remove(file);
        var count = _db.SaveChanges(); // TODO 2021-06-07 No apparent reason why SaveChangesAsync won't work here
        return count >= 1 ? true : false;
    }

    /// <summary>
    /// Creates a new tag.
    /// </summary>
    /// <param name="tag">Text of tag to create.</param>
    /// <returns>Bool indicating success/failure.</returns>
    public async Task<(bool success, Tag? tagInDatabase)> CreateTag(string tag)
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
            var count = _db.SaveChanges(); // TODO 2021-06-07 No apparent reason why SaveChangesAsync won't work here
            return count < 1 ? (false, null) : (true, newTag);
        }
        else
        {
            return (true, existsCheck);
        }
    }

    /// <summary>
    /// Deletes a tag entirely including tagged files and the tag itself.
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
            var result = await DisassociateTagFromEverything(existsCheck.Id);

            if (result == true)
            {
                _db.Tags.Remove(existsCheck);
                var count = _db.SaveChanges(); // TODO 2021-06-07 No apparent reason why SaveChangesAsync won't work here
                return count == 1 ? true : false;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Disassociates a tag from files but doesn't delete the tag itself.
    /// </summary>
    /// <param name="tagId">Id of tag to have associations removed from.</param>
    /// <returns>Bool indicating a likely success if at least one association deleted.</returns>
    public async Task<bool> DisassociateTagFromEverything(Guid tagId)
    {
        var appliedTags = _db.AppliedTags.Where(x => x.TagRefId.Id == tagId);
        foreach (var x in appliedTags)
        {
            _db.AppliedTags.Remove(x);
        }

        var count = _db.SaveChanges(); // TODO 2021-06-07 No apparent reason why SaveChangesAsync won't work here
        return count >= 1 ? true : false;
    }

    /// <summary>
    /// Links a tag to a database, creating tag if needed.
    /// </summary>
    /// <param name="fileId">Id of file to apply tag to.</param>
    /// <param name="tag">Tag to apply to file.</param>
    /// <returns>Bool indicating success/failure.</returns>
    public async Task<bool> LinkTagToListing(string fileId, string tag)
    {
        var fileIdInDatabase = ListingInDatabase(fileId);

        if (fileIdInDatabase.present == true)
        {
            var tagResult = await CreateTag(tag);

            if (tagResult.success == true)
            {
                #pragma warning disable CS8601
                _db.AppliedTags.Add(new AppliedTag { Id = Guid.NewGuid(), FileRefId = fileIdInDatabase.file, TagRefId = tagResult.tagInDatabase});
                var count = _db.SaveChanges(); // TODO 2021-06-07 No apparent reason why SaveChangesAsync won't work here
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