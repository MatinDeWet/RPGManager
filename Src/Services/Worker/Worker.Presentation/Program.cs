using Worker.Application;
using Worker.infrastructure;
using Worker.Presentation.Common.DIExtensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

bool isDevelopmentOrStaging = builder.Environment.IsDevelopment() || builder.Environment.IsStaging();

builder.Services.AddWorkerApplication();
builder.Services.AddWorkerInfrastructure(builder.Configuration, isDevelopmentOrStaging);
builder.Services.AddDashboardAuthentication(builder.Configuration);

WebApplication app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseWorkerDashboard();

app.RegisterRecurringJobs();

await app.RunAsync();
