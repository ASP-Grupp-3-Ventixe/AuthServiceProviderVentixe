using Grpc.Net.Client.Web;
using Microsoft.OpenApi.Models;
using Presentation;
using Presentation.Services;
using Swashbuckle.AspNetCore.Filters;
using IAuthService = Presentation.Services.IAuthService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1.0",
        Title = "Auth Service API Documentation",
        Description = "Official Documentation for Auth Service Provider API."
    });
    o.EnableAnnotations();
    o.ExampleFilters();
});

builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll",
    policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddHttpClient();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddGrpcClient<AccountGrpcService.AccountGrpcServiceClient>(x =>
{
    x.Address = new Uri(builder.Configuration["Providers:AccountServiceProvider"]!);
})
    .ConfigurePrimaryHttpMessageHandler(() => new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));

var app = builder.Build();
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ventixe AuthServiceProvider");
    c.RoutePrefix = string.Empty;
});

app.UseHsts();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
