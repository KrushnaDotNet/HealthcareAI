using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

////AddMcpServer() Configure this application as an MCP Server.
//// WithHttpTransport() Configure the HTTP transport for the MCP Server.
//// Find classes marked as MCP tool types and register their tools.

builder.Services
    .AddMcpServer()
    .WithHttpTransport(options =>
    {
        options.Stateless = true;
    })
    .WithToolsFromAssembly();

var app = builder.Build();

app.MapMcp();

app.Run("http://localhost:3001");

/// <summary>
/// The official SDK supports WithHttpTransport() and MapMcp() for an ASP.NET Core MCP server
/// marks the class as containing MCP tools.
/// becomes an MCP capability that a client can discover and call.
/// </summary>

[McpServerToolType]
public static class HospitalTools
{
    [McpServerTool]
    [Description("Returns the name of the hospital.")]
    public static string GetHospitalName()
    {
        return "HealthcareAI Hospital";
    }

    [McpServerTool]
    [Description("Returns patient details for the specified patient ID.")]
    public static string GetPatientById(
        [Description("The unique patient ID. Example: P1001")]
        string patientId)
    {
        if (patientId == "P1001")
        {
            return "Patient: John Doe, Age: 45";
        }

        if (patientId == "P1002")
        {
            return "Patient: Jane Smith, Age: 32";
        }

        return $"Patient with ID {patientId} was not found.";
    }

    [McpServerTool]
    [Description("Returns the patient ID for the specified patient name.")]
    public static string GetPatientIdByName(
        [Description("The patient name. Example: John Doe")]
        string patientName)
    {
        if (patientName.Equals(
            "John Doe",
            StringComparison.OrdinalIgnoreCase))
        {
            return "P1001";
        }

        if (patientName.Equals(
            "Jane Smith",
            StringComparison.OrdinalIgnoreCase))
        {
            return "P1002";
        }

        return $"Patient named {patientName} was not found.";
    }
}