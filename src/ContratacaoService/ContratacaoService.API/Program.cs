using ContratacaoService.Application;
using ContratacaoService.Infra.Data.Persistence;
using ContratacaoService.Infra.IoC;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ContratacaoService API", Version = "v1" });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

var maxRetries = 5;  
var retryCount = 0;
while (retryCount < maxRetries)
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ContratacaoDbContext>();
            dbContext.Database.Migrate();
        }
        break;
    }
    catch (Exception ex)
    {
        retryCount++;
        if (retryCount >= maxRetries)
        {
            app.Logger.LogError(ex, "Falha ao aplicar migrations após {MaxRetries} tentativas", maxRetries);
            throw;
        }
        app.Logger.LogWarning(ex, "Erro ao aplicar migrations. Tentando novamente em 2 segundos... ({RetryCount}/{MaxRetries})", retryCount, maxRetries);
        await Task.Delay(2000);
    }
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ContratacaoService API v1"));

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
