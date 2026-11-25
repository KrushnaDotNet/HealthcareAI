using HealthcareAI.Data;
using HealthcareAI.Plugins;
using HealthcareAI.RAG;
using HealthcareAI.Services;
using Microsoft.EntityFrameworkCore;
using Qdrant.Client;
using Qdrant.Client.Grpc;

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

// Configure Qdrant client
// Note: Ensure that Qdrant is running on localhost:6334 (6333 → HTTP API) or update the host and port accordingly.
// You can also configure Qdrant settings in appsettings.json and read them here if needed.
// Example: builder.Configuration.GetSection("QdrantSettings").Get<QdrantSettings>();
// 6334 → gRPC
// Collection name: healthcare_documents
// Vector size:     384
// Distance metric: Cosine

builder.Services.AddSingleton<QdrantClient>(_ =>
    new QdrantClient("localhost", 6334));

builder.Services.AddSingleton<QdrantVectorStore>();
builder.Services.AddSingleton<QdrantIngestionService>();
builder.Services.AddSingleton<QdrantVectorSearch>();

var app = builder.Build();

////var qdrantClient = app.Services.GetRequiredService<QdrantClient>();

////await qdrantClient.CreateCollectionAsync(
////    "healthcare_documents",
////    new VectorParams
////    {
////        Size = 768,
////        Distance = Distance.Cosine
////    });

var doco = @"C:\Users\krush\source\repos\HealthcareAI\RAG\Documents\AppointmentPolicy.txt";

app.MapGet("/test-qdrant-ingestion", async (
    DocumentLoader documentLoader,
    QdrantIngestionService ingestionService) =>
{
    var filePath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "RAG",
        "Documents",
        doco);

    var text = documentLoader.LoadDocument(filePath);

    await ingestionService.IngestDocumentAsync(
        text,
        "AppointmentPolicy.txt");

    return Results.Ok("Document ingested into Qdrant successfully.");
});

app.MapGet("/test-qdrant-search", async (
    EmbeddingService embeddingService,
    QdrantVectorSearch vectorSearch) =>
{
    var question =
        "Can I cancel my appointment 12 hours before?";

    var embedding =
        await embeddingService.GenerateEmbeddingAsync(question);

    var results =
        await vectorSearch.SearchAsync(embedding, 3);

    return Results.Ok(new
    {
        Question = question,
        Results = results
    });
});

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
