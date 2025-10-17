var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add Health Checks
builder.Services.AddHealthChecks();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("ApiPolicy", policyBuilder =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        
        if (origins?.Contains("*") == true)
        {
            // Desarrollo: Permite cualquier origen
            policyBuilder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
        }
        else
        {
            // Producción: Solo orígenes específicos
            policyBuilder.WithOrigins(origins ?? Array.Empty<string>())
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
        }
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "DevOps API",
        Version = "v1",
        Description = "API REST automatizada con CI/CD, Terraform y despliegue en Azure Container Apps",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "DevOps Team",
            Email = "devops@example.com",
            Url = new Uri("https://github.com/your-org/api-devops")
        },
        License = new Microsoft.OpenApi.Models.OpenApiLicense
        {
            Name = "Uso Interno"
        }
    });

    // Habilitar comentarios XML
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DevOps API v1");
        c.RoutePrefix = string.Empty; // Swagger UI en raíz
        c.DocumentTitle = "DevOps API - Swagger UI";
    });
}
else
{
    // Swagger disponible también en producción (configurable)
    var enableSwaggerInProduction = builder.Configuration.GetValue<bool>("EnableSwaggerInProduction", false);
    if (enableSwaggerInProduction)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "DevOps API v1");
            c.RoutePrefix = "swagger";
        });
    }
}

app.UseHttpsRedirection();

app.UseCors("ApiPolicy");

app.MapControllers();

// Health Check Endpoints
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false, // Solo verifica que el proceso está vivo
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
