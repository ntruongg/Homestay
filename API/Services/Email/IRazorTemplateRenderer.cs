namespace API.Services.Email;

public interface IRazorTemplateRenderer
{
    Task<string> RenderTemplateAsync<TModel>(string templateName, TModel model);
}
