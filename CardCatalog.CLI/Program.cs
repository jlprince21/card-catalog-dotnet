using System;
using CardCatalog.Core;
using System.Threading.Tasks;

namespace CardCatalog.CLI
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            try
            {
                var y = new Class1();
                var x = await y.CreateListing();
                Console.WriteLine("x: " + x);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error " + ex.Message + ex.InnerException);
            }
        }
    }
}
