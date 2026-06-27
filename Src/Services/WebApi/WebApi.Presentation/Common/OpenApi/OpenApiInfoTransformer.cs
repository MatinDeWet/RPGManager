using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace WebApi.Presentation.Common.OpenApi;

/// <summary>
/// Sets the document-level <c>Info</c> (title, version, description) on the generated OpenAPI
/// document. Stable, descriptive Info improves the naming/namespacing of generated clients.
/// </summary>
internal sealed class OpenApiInfoTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info ??= new OpenApiInfo();
        document.Info.Title = "RPGManager API";
        document.Info.Version = context.DocumentName;
        document.Info.Description = "HTTP API for RPGManager — manage worlds, campaigns, members, and invitations.";

        return Task.CompletedTask;
    }
}
