using ModelContextProtocol.Client;

Console.WriteLine("1. Client application started.");

var clientTransport = new HttpClientTransport(
    new HttpClientTransportOptions
    {
        Endpoint = new Uri("http://localhost:3001")
    });

Console.WriteLine("2. Transport created.");

await using var mcpClient = await McpClient.CreateAsync(clientTransport);

Console.WriteLine("3. Connected to MCP Server.");

var tools = await mcpClient.ListToolsAsync();

Console.WriteLine("\nTool Details:");

Console.WriteLine($"4. Tools discovered: {tools.Count}");

foreach (var tool in tools)
{
    Console.WriteLine($"\nName: {tool.Name}");
    Console.WriteLine($"Description: {tool.Description}");
    Console.WriteLine($"Input Schema: {tool.JsonSchema}");
}

// Call the MCP tool
Console.WriteLine("\n6. Calling get_hospital_name...");

var result = await mcpClient.CallToolAsync(
    "get_patient_by_id",
    new Dictionary<string, object?>
    {
        ["patientId"] = "P1001"
    });

Console.WriteLine("7. Tool response:");

foreach (var content in result.Content)
{
    Console.WriteLine(content);
}

Console.WriteLine("\n8. Client finished.");

Console.ReadKey();