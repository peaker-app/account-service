using System.Text.Json.Serialization;
using AccountService.Application;
using AccountService.Infrastructure;
using AccountService.Infrastructure.Persistence;
using Common.API.Health;
using Common.API.Middlewares;
using Common.API.Security;
using Common.Application.Abstractions;
using Common.Infrastructure.Observability;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddCommonSerilog("account-service");
builder.AddCommonOpenTelemetry("account-service");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();
builder.Services.AddCommonJwtAuthentication(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks().AddDbContextCheck<AccountDbContext>();

WebApplication app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapCommonHealthChecks();

await app.RunAsync();

public partial class Program;
