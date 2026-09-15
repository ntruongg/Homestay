using Microsoft.Extensions.Logging;
using RazorLight;

namespace API.Services.Email;

public sealed class RazorTemplateRenderer : IRazorTemplateRenderer
{
    private readonly RazorLightEngine _engine;
    private readonly ILogger<RazorTemplateRenderer> _logger;

    public RazorTemplateRenderer(ILogger<RazorTemplateRenderer> logger)
    {
        _logger = logger;

        var baseDir = AppContext.BaseDirectory;
        var templatesDir = Path.Combine(baseDir, "Templates", "Emails");

        if (!Directory.Exists(templatesDir))
        {
            var currentDir = Directory.GetCurrentDirectory();
            var candidate = Path.Combine(currentDir, "Templates", "Emails");
            if (Directory.Exists(candidate))
            {
                templatesDir = candidate;
            }
            else
            {
                // Create directory if not existing to prevent builder failure
                Directory.CreateDirectory(templatesDir);
            }
        }

        _logger.LogInformation("Initializing RazorLightEngine with templates directory: {TemplatesDir}", templatesDir);

        _engine = new RazorLightEngineBuilder()
            .UseFileSystemProject(templatesDir)
            .UseMemoryCachingProvider()
            .SetOperatingAssembly(typeof(RazorTemplateRenderer).Assembly)
            .Build();
    }

    public async Task<string> RenderTemplateAsync<TModel>(string templateName, TModel model)
    {
        var key = templateName.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase)
            ? templateName
            : $"{templateName}.cshtml";

        try
        {
            return await _engine.CompileRenderAsync(key, model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compile/render Razor template: {TemplateKey}", key);
            throw;
        }
    }
}
