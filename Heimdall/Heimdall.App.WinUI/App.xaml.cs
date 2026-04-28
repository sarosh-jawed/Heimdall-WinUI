using Heimdall.Infrastructure.Matching;
using Heimdall.Infrastructure.Bragi;
using Heimdall.BragiCore.Extraction;
using Heimdall.BragiCore.Export;
using Heimdall.BragiCore.Configuration;
using Heimdall.BragiCore.Categorization;
using Heimdall.Infrastructure.Html;
using Heimdall.Infrastructure.Csv;
using System;
using System.IO;
using Heimdall.Application.Configuration;
using Heimdall.Application.Contracts;
using Heimdall.Application.Workflow;
using Heimdall.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Serilog;

namespace Heimdall.App.WinUI;

/// <summary>
/// Application startup for the Heimdall WinUI desktop app.
/// This class owns configuration loading, dependency injection, and application-level logging.
/// </summary>
public partial class App : Microsoft.UI.Xaml.Application
{
    private IHost? _host;
    private Window? _window;

    public App()
    {
        InitializeComponent();

        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            Log.CloseAndFlush();
            _host?.Dispose();
        };
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            _host = CreateHost();
            _host.Start();

            ILogger<App> logger = _host.Services.GetRequiredService<ILogger<App>>();
            logger.LogInformation("Application started");
            logger.LogInformation("Config loaded");
            logger.LogInformation("Services registered");

            _window = _host.Services.GetRequiredService<MainWindow>();
            _window.Activate();
        }
        catch (Exception ex)
        {
            ShowStartupFailure(ex);
        }
    }

    private static IHost CreateHost()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "config.json");

        var configLoader = new HeimdallConfigLoader();
        var config = configLoader.Load(configPath);

        var configValidator = new HeimdallConfigValidator();
        configValidator.ValidateAndThrow(config);

        var logFolder = ResolveLogFolder(config);
        Directory.CreateDirectory(logFolder);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                Path.Combine(logFolder, config.Logging.LogFileNameTemplate),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: config.Logging.RetainedFileCountLimit,
                shared: true)
            .CreateLogger();

        return Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddSerilog(Log.Logger, dispose: false);
            })
            .ConfigureServices(services =>
            {
                services.AddSingleton(config);
                services.AddSingleton<IHeimdallConfigLoader, HeimdallConfigLoader>();
                services.AddSingleton<IHeimdallConfigValidator, HeimdallConfigValidator>();

                services.AddSingleton<ISummaryExtractor, SummaryExtractor>();
                services.AddSingleton<ICsvSchemaValidator, CsvSchemaValidator>();
                services.AddSingleton<ICsvBookRecordReader, CsvBookRecordReader>();

                services.AddSingleton(new BragiCoreOptions());
                services.AddSingleton<SubjectExtractionService>();
                services.AddSingleton<CategorizationService>();
                services.AddSingleton<TextExportService>();
                services.AddSingleton<IBragiSubjectListGenerator, BragiSubjectListGenerator>();
                services.AddSingleton<ICategoryFileDetector, CategoryFileDetector>();
                services.AddSingleton<ISubjectListFolderReader, SubjectListFolderReader>();
                services.AddSingleton<IBookCategoryMatcher, BookCategoryMatcher>();

                services.AddSingleton<WizardSessionStore>();
                services.AddSingleton<IWorkflowOrchestrator, WorkflowOrchestrator>();

                services.AddTransient<MainWindow>();
            })
            .Build();
    }

    private static string ResolveLogFolder(HeimdallConfig config)
    {
        var rootFolder = config.Logging.UseDocumentsFolder
            ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        return Path.Combine(rootFolder, config.Logging.LogFolderName);
    }

    private void ShowStartupFailure(Exception exception)
    {
        try
        {
            Log.Fatal(exception, "Heimdall failed during startup");
        }
        finally
        {
            Log.CloseAndFlush();
        }

        _window = new Window
        {
            Title = "Heimdall startup error",
            Content = new StackPanel
            {
                Spacing = 12,
                Padding = new Thickness(24),
                Children =
                {
                    new TextBlock
                    {
                        Text = "Heimdall could not start.",
                        FontSize = 22,
                        FontWeight = FontWeights.SemiBold
                    },
                    new TextBlock
                    {
                        Text = "The application configuration or startup services could not be loaded. Check Documents\\Heimdall\\Logs for technical details.",
                        TextWrapping = TextWrapping.Wrap
                    },
                    new TextBlock
                    {
                        Text = exception.Message,
                        TextWrapping = TextWrapping.Wrap
                    }
                }
            }
        };

        _window.Activate();
    }
}




