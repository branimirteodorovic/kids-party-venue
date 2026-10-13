using HealthChecks.UI.Client;
using KidsPartyVenue.API.Extensions;
using KidsPartyVenue.API.Middleware;
using KidsPartyVenue.Application;
using KidsPartyVenue.Infrastructure;
using KidsPartyVenue.Infrastructure.Configuration;
using KidsPartyVenue.Presentation;
using KidsPartyVenue.Presentation.Endpoints;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Npgsql;
using Serilog;
using Scalar.AspNetCore;
using KidsPartyVenue.Infrastructure.BackgroundJobs;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();

// TODO: Is swagger needed when we have OpenApi?
//builder.Services.AddSwaggerDocumentation();

builder.Services.AddApplication(KidsPartyVenue.Application.AssemblyReference.Assembly);

string databaseConnectionString = builder.Configuration.GetConnectionStringOrThrow("Database");

builder.Services.AddInfrastructure(
    builder.Configuration,
    databaseConnectionString);

builder.Services.AddPresentation();

builder.Services.AddHealthChecks()
    .AddNpgSql(sp => sp.GetRequiredService<NpgsqlDataSource>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapGet("/", () => Results.Redirect("/scalar"));

    app.ApplyMigrations();
}

app.UseBackgroundJobsDashboard(app.Configuration);
app.RegisterRecurringJobs();

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseLogContextTraceLogging();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseAuthentication();

app.UseAuthorization();

app.MapEndpoints();

app.Run();
