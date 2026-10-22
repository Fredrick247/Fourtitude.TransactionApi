using Fourtitude.TransactionApi.Api.Models;
using Fourtitude.TransactionApi.Api.Services;
using Fourtitude.TransactionApi.Api.Validation;
using Microsoft.AspNetCore.Mvc;
using Fourtitude.TransactionApi.Api.Logging;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.Configure<List<Partner>>(builder.Configuration.GetSection("Partners"));
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = _ =>
            new OkObjectResult(SubmitTransactionResponse.Failure(ErrorMessages.InvalidRequest));
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IPartnerService, PartnerService>();
builder.Services.AddSingleton<ISignatureService, SignatureService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ITransactionValidator, TransactionValidator>();
builder.Services.AddSingleton<IDiscountService, DiscountService>();
builder.Logging.ClearProviders();
builder.Logging.AddLog4Net("log4net.config");
builder.Services.Configure<LogEncryptionOptions>(builder.Configuration.GetSection(LogEncryptionOptions.SectionName));
builder.Services.AddSingleton<ILogEncryptor, AesLogEncryptor>();
builder.Services.AddSingleton<SensitiveDataProtector>();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
