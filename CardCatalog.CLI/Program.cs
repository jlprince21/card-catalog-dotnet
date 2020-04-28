using CardCatalog.Core;
using System;
using System.Threading.Tasks;

namespace CardCatalog.CLI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var y = new FileProcessing();

                // var createListingResult = await y.CreateListing(checksum: "test", fileName: "test", fileSize: 50, filePath: "blah");
                // Console.WriteLine("createListingResult: " + createListingResult);

                // var hashFileResult = y.HashFile("/Users/jlprince21/Desktop/test.txt");
                // Console.WriteLine("hashFileResult: " + hashFileResult.success + " " +hashFileResult.hash);

                // var createTagResult = await y.CreateTag("test");
                // Console.WriteLine("createTagResult: " + createTagResult);

                // await y.ScanFiles("/Users/jlprince21/Desktop/scantest");
                // await y.ScanFiles("/Volumes/alexandria/luke");

                // await y.DeleteOrphans(deleteListingOnOrphanFound: true);

                await y.LinkTagToListing("7C8D2C05-3037-4058-9C95-42A6400A6D48", "iAmANewTag");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error " + ex.Message + ex.InnerException);
            }
        }
    }
}
