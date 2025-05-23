using Grpc.Net.Client.Web;
using Presentation;
using Presentation.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
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
});
    
var app = builder.Build();
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ventixe AuthServiceProvider API");
    c.RoutePrefix = string.Empty; 
});

app.UseHsts();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
