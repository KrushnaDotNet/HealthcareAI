using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OllamaSharp;

Console.WriteLine("Starting MCP Agent Demo...");

// ------------------------------------
// 1. Connect to Ollama using IChatClient
// ------------------------------------

IChatClient chatClient =
    Microsoft.Extensions.AI.ChatClientBuilderChatClientExtensions
        .AsBuilder(
            new OllamaApiClient(
                new Uri("http://localhost:11434"),
                "qwen2.5:3b"))
        .UseFunctionInvocation()
        .Build();

Console.WriteLine("Ollama connected.");


// ------------------------------------
// 2. Connect to MCP Server
// ------------------------------------

var clientTransport = new HttpClientTransport(
    new HttpClientTransportOptions
    {
        Endpoint = new Uri("http://localhost:3001")
    });

await using var mcpClient =
    await McpClient.CreateAsync(clientTransport);

Console.WriteLine("MCP Server connected.");


// ------------------------------------
// 3. Discover MCP Tools
// ------------------------------------

var mcpTools = await mcpClient.ListToolsAsync();

Console.WriteLine($"\nMCP tools discovered: {mcpTools.Count}");

foreach (var tool in mcpTools)
{
    Console.WriteLine($"- {tool.Name}");
}


// ------------------------------------
// 4. Ask LLM using native tools
// ------------------------------------

var question =
    "Find the patient ID for John Doe using the available tools. Then use that patient ID to get his complete patient details.";

Console.WriteLine($"\nUser question:");
Console.WriteLine(question);

Console.WriteLine("\nAsking LLM with MCP tools...");

var response = await chatClient.GetResponseAsync(
    question,
    new ChatOptions
    {
        Tools = [.. mcpTools]
    });

Console.WriteLine("\nAgent execution completed.");

// ------------------------------------
// 5. Display response
// ------------------------------------

Console.WriteLine("\nLLM response:");

Console.WriteLine(response);

Console.WriteLine("\nLLM response:");

foreach (var message in response.Messages)
{
    Console.WriteLine($"\nMessage Role: {message.Role}");

    foreach (var content in message.Contents)
    {
        Console.WriteLine($"Content Type: {content.GetType().Name}");

        if (content is FunctionCallContent functionCall)
        {
            Console.WriteLine($"Tool Name: {functionCall.Name}");
            Console.WriteLine($"Arguments: {System.Text.Json.JsonSerializer.Serialize(functionCall.Arguments)}");
        }
        else if (content is FunctionResultContent functionResult)
        {
            Console.WriteLine($"Tool Result: {functionResult.Result}");
        }
        else
        {
            Console.WriteLine($"Content: {content}");
        }
    }
}

// ------------------------------------
// 6. Keep console open
// ------------------------------------

Console.WriteLine("\nPress any key to exit.");

Console.ReadKey();