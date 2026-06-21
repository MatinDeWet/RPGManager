using WebApi.Presentation.Common.DIExtensions;
using WebApi.Presentation.Common.Middleware;
using WebApi.Presentation.Endpoints.UserEndpoints;
using WebApi.Presentation.Endpoints.WorldEndpoints;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);

WebApplication app = builder.Build();

app.UseExceptionHandler();

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseApiDocumentation();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<CurrentUserMiddleware>();

app.MapUserEndpoints();
app.MapWorldEndpoints();

await app.ApplyDatabaseMigrationsAsync();

await app.RunAsync();
