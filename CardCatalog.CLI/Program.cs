using System;
using CardCatalog.Core;
using System.Threading.Tasks;

namespace CardCatalog.CLI
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var y = new FileProcessing();

                var createListingResult = await y.CreateListing(checksum: "test", fileName: "test", fileSize: 50, filePath: "blah");
                Console.WriteLine("createListingResult: " + createListingResult);

                var hashFileResult = y.HashFile("/Users/jlprince21/Desktop/test.txt");
                Console.WriteLine("hashFileResult: " + hashFileResult.success + " " +hashFileResult.hash);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error " + ex.Message + ex.InnerException);
            }
        }
    }
}
