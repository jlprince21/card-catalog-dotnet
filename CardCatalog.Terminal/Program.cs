using Microsoft.EntityFrameworkCore;
using CardCatalog.Core;
using System;
using System.Threading.Tasks;

namespace CardCatalog.Terminal
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<CardCatalogContext>();
                optionsBuilder.UseSqlite("Data Source=cc_dev.db");

                CardCatalogContext dbContext = new CardCatalogContext(optionsBuilder.Options);

                var y = new FileProcessing(dbContext);

                await y.ScanFiles("/some/path");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error " + ex.Message + ex.InnerException);
            }
        }
    }
}
