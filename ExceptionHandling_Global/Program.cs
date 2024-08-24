using ExceptionHandling_Global.Handler;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Annotations;

using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;


// Install-Package Microsoft.ApplicationInsights.AspNetCore



var builder = WebApplication.CreateBuilder(args);

#region applicationInsight

bool isYahoo = false;
Boolean.TryParse(builder.Configuration["isYahoo"], out isYahoo);
string appInsightString = (isYahoo) ? "AppInsightYahoo" : "AppInsightVS";

builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var applicationInsightsString = builder.Configuration["ConnectionStrings:" + appInsightString];
builder.Services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
{
    ConnectionString = applicationInsightsString,
    EnableActiveTelemetryConfigurationSetup = true,
});
var Logging = builder.Configuration.GetSection("Logging");      //appSettings.json

var logLevelEnv = builder.Configuration["LOG_LEVEL"]?.ToLower() ?? "information";
builder.Logging.SetMinimumLevel(Enum.Parse<LogLevel>(logLevelEnv, true));

#endregion


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//register global exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

//register Caching
builder.Services.AddMemoryCache();


builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    c.EnableAnnotations();
});

//set logLevel
LogLevel logLevel = Enum.Parse<LogLevel>(logLevelEnv, true);
builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>("", logLevel);



var app = builder.Build();


// Configure the HTTP request pipeline.

var _logger = app.Logger;
_logger.LogCritical($"*** ExceptionHandling_Global is starting, Log Level: {logLevelEnv} ***");
_logger.LogCritical($"ExceptionHandling_Global > app.Environment: {app.Environment} ");
_logger.LogCritical($"ExceptionHandling_Global > app.Environment: {app.Environment.IsDevelopment()} ");


bool swaggerUI = app.Environment.IsDevelopment() ? true : true;     //on azure: IsDevelopment()=false
if (swaggerUI)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI as the default page
    });
}

app.UseExceptionHandler( _ => { });

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();



app.MapGet("api/foo", () =>
{
    //traceId: 4e0f77870da7ce0da3ea882f005a9aaa
    //Program: Request starting HTTP/1.1 GET https://exceptionhandling-global-20240817.azurewebsites.net/api/foo - - -
    //Program: Executing endpoint 'HTTP: GET api/foo'
    //GlobalExceptionHandler: Exception: error happens from MapGet foo, MachineName: WEBWK000002, TraceId: 00-4e0f77870da7ce0da3ea882f005a9aaa-297b8515dd85dbb6-00
    //GlobalExceptionHandler: Setting HTTP status code 400.
    //GlobalExceptionHandler: Writing value of type 'ProblemDetails' as Json.
    //Program: Request finished HTTP/1.1 GET https://exceptionhandling-global-20240817.azurewebsites.net/api/foo - 400 - application/problem+json 2.4156ms

    throw new InvalidOperationException("error happens from MapGet foo");
});


app.MapGet("api/foo/{id}", async (int id) =>
{
    if (id < 1)
    {
        throw new ArgumentOutOfRangeException(nameof(id), "The argument must be greater than 0");
    }

    return await Task.FromResult(id);
})
.WithMetadata(new SwaggerOperationAttribute
{
    Summary = "'id: 0' will return 400 client error ",
    Description = "This method returns an 400 error.\n\n ** Client error **\n\n * Client error *\n\n - This is a list item\n\n - return 400 error\n\n"
}); 



app.Run();
