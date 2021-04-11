using CardCatalog.Core;
using CommandLine;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace CardCatalog.Terminal
{
    public class Program
    {
        public class Options
        {
            [Option('o', "orphans", Default = false, HelpText = "Finds orphans. Prompts user if they should be deleted from database when found.")]
            public bool orphans { get; set; }

            [Option('h', "hash", Required = false, Default = "", HelpText = "Hash all files")]
            public string hash { get; set; }
        }

        public static async Task Main(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CardCatalogContext>();
            optionsBuilder.UseNpgsql(Environment.GetEnvironmentVariable("CARD_CATALOG_DB_CONNECTION"));
            CardCatalogContext dbContext = new CardCatalogContext(optionsBuilder.Options);
            var y = new FileProcessing(dbContext);

            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(async o =>
                {
                    if (o.orphans == true)
                    {
                        var result = PromptUser("Delete found orphans from database (Y/N)? ");

                        if (result != string.Empty && result.Length == 1)
                        {
                            var uppercase = result.ToUpper();
                            if (uppercase == "Y")
                            {
                                await y.DeleteOrphans(true);
                            }
                            else if (uppercase == "N")
                            {
                                await y.DeleteOrphans(false);
                            }
                            else
                            {
                                ExitWithMessage("Invalid response. Exiting.");
                            }
                        }
                        else
                        {
                            ExitWithMessage("Invalid response. Exiting.");
                        }
                    }

                    if (o.hash != "")
                    {
                        var tokens = o.hash.Split(',');

                        if (tokens.Length == 1 && tokens[0] != String.Empty)
                        {
                            var path = tokens[0];
                            Console.WriteLine("Hashing files starting at: " + path);

                            try
                            {
                                await y.ScanFiles(path);
                                Environment.Exit(0);
                            }
                            catch (Exception ex)
                            {
                                ExitWithMessage("Error, exiting: " + ex.Message + ex.InnerException);
                            }
                        }
                        else
                        {
                            ExitWithMessage("Please provide path of files to hash.");
                        }
                    }
                });
        }

        public static string PromptUser(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }

        public static void ExitWithMessage(string message)
        {
            Console.WriteLine(message);
            Environment.Exit(0);
        }
    }
}
