// System namespaces
using System;
using System.Threading.Tasks;

// Third-party namespaces (NuGet packages)
using CommandLine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;

// Project-specific namespaces
using CardCatalog.Core;
using CardCatalog.Core.Services;

namespace CardCatalog.Terminal;

public class Program
{
    public class Options
    {
        [Option('m', "missing", Default = false, HelpText = "Finds files missing from the database. Prompts user if they should be deleted from database when found.")]
        public bool missing { get; set; }

        [Option('h', "hash", Required = false, Default = "", HelpText = "Hash all files")]
        public string hash { get; set; }
    }

    public static async Task Main(string[] args)
    {
        // Create a HostBuilder and configure services for dependency injection
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<IClock>(SystemClock.Instance);

                // Register services here
                services.AddScoped<ITimeService, TimeService>();
            })
            .Build();

        // Get services and use them
        var timeService = host.Services.GetRequiredService<ITimeService>(); // for NodaTime

        var optionsBuilder = new DbContextOptionsBuilder<CardCatalogContext>();
        optionsBuilder.UseNpgsql(Environment.GetEnvironmentVariable("CARD_CATALOG_DB_CONNECTION"));
        CardCatalogContext dbContext = new CardCatalogContext(optionsBuilder.Options);
        var y = new FileProcessing(dbContext, timeService);

        CommandLine.Parser.Default.ParseArguments<Options>(args)
            .WithParsed<Options>(async o =>
            {
                if (o.missing == true)
                {
                    var result = PromptUser("Delete files missing from the database (Y/N)? ");

                    if (result != string.Empty && result.Length == 1)
                    {
                        var uppercase = result.ToUpper();
                        if (uppercase == "Y")
                        {
                            await y.DeleteMissing(true);
                        }
                        else if (uppercase == "N")
                        {
                            await y.DeleteMissing(false);
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
