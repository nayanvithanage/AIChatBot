using InEightAIChatbot.Core.Interfaces;
using InEightAIChatbot.Infrastructure.Providers;
using InEightAIChatbot.Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Pgvector.Npgsql;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS for DMS integration
builder.Services.AddCors(options =>
{
    options.AddPolicy("DMS", policy =>
    {
        policy.WithOrigins(
                // Development - React widget
                "http://localhost:5173",
                "http://localhost:5174",
                "http://localhost:5175",
                "https://localhost:5173",
                // Development - DMS
                "http://localhost:44300",
                "https://localhost:44300",
                "http://localhost:52291",
                "https://localhost:52291",
                "http://localhost:5000",
                "https://localhost:5001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!))
        };
    });

// PostgreSQL with pgvector
var pgConnectionString = builder.Configuration.GetConnectionString("PostgreSQL")!;
var dataSourceBuilder = new NpgsqlDataSourceBuilder(pgConnectionString);
dataSourceBuilder.UseVector();
builder.Services.AddSingleton(dataSourceBuilder.Build());

// Configure AI Providers based on settings
var aiProvider = builder.Configuration["AI:Provider"];
var vectorProvider = builder.Configuration["AI:VectorStore"];

builder.Services.Configure<OllamaSettings>(
    builder.Configuration.GetSection("AI:Ollama"));

builder.Services.AddHttpClient<OllamaProvider>();

builder.Services.AddSingleton<IAIProvider>(sp =>
{
    return aiProvider switch
    {
        "Ollama" => sp.GetRequiredService<OllamaProvider>(),
        "AzureOpenAI" => new AzureOpenAIProvider(),
        _ => throw new InvalidOperationException($"Unknown AI provider: {aiProvider}")
    };
});

builder.Services.AddSingleton<IVectorSearchProvider>(sp =>
{
    return vectorProvider switch
    {
        "PgVector" => new PgVectorProvider(sp.GetRequiredService<NpgsqlDataSource>()),
        "AzureAISearch" => new AzureAISearchProvider(),
        _ => throw new InvalidOperationException($"Unknown vector provider: {vectorProvider}")
    };
});

// Core services
builder.Services.AddSingleton<RAGOrchestrator>();
builder.Services.AddHostedService<MetadataSyncService>();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("DMS");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
