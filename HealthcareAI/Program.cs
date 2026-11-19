using HealthcareAI.Data;
using HealthcareAI.Plugins;
using HealthcareAI.RAG;
using HealthcareAI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<ChatHistoryStore>();
builder.Services.AddScoped<LongTermMemoryStore>();

builder.Services.AddDbContext<HealthcareDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("HealthcareDb")));
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<SemanticKernelService>();

builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<DocumentLoader>();
builder.Services.AddSingleton<TextChunker>();
builder.Services.AddHttpClient<EmbeddingService>();
builder.Services.AddSingleton<VectorSimilarityService>();
builder.Services.AddScoped<QuestionRouter>();
builder.Services.AddScoped<RagPlugin>();
builder.Services.AddScoped<AgentRequestContext>();
builder.Services.AddScoped<IAgentAuthorizationService, AgentAuthorizationService>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
