using HealthcareAI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;

namespace HealthcareAI.Plugins
{
    public class HealthcarePlugin
    {
        private readonly IPatientService _patientService;

        private readonly IAppointmentService _appointmentService;

        private readonly AgentRequestContext _requestContext;

        private readonly IAgentAuthorizationService _agentAuthorizationService;

        private readonly ILogger<HealthcarePlugin> _logger;

        public HealthcarePlugin(IPatientService patientService, IAppointmentService appointmentService,
            AgentRequestContext requestContext, IAgentAuthorizationService agentAuthorizationService, ILogger<HealthcarePlugin> logger)
        {
            _patientService = patientService;
            _appointmentService = appointmentService;
            _requestContext = requestContext;
            _agentAuthorizationService = agentAuthorizationService;
            _logger = logger;
        }

        [KernelFunction]
        [Description("Provides emergency medical guidance when a user reports a serious or potentially dangerous symptom.")]
        public string GetEmergencyGuidelines([Description("The symptom reported by the user.")] string symptom)
        {
            return $"For symptoms such as '{symptom}', seek immediate medical attention if they are severe, sudden, or worsening.";
        }

        [KernelFunction("SearchPatientStatic")]
        [Description("Searches for a patient by patient name.")]
        public string SearchPatient([Description("The name of the patient to search for.")] string patientName)
        {
            return $"Patient '{patientName}' was found. Patient ID: P1001.";
        }

        [KernelFunction]
        [Description("Returns all patients available in the database.")]
        public async Task<string> GetAllPatientsAsync()
        {
            if(!_agentAuthorizationService.IsToolAllowed(_requestContext.AccessLevel, nameof(GetAllPatientsAsync)))
            {
                return "Access denied. You do not have permission to access this tool.";
            }

            if (!_agentAuthorizationService.IsToolAllowed( _requestContext.AccessLevel,"GetAllPatientsAsync"))
            {
                _logger.LogWarning(
                    "Tool authorization denied. UserId: {UserId}, AccessLevel: {AccessLevel}, Tool: {ToolName}",
                    _requestContext.UserId,
                    _requestContext.AccessLevel,
                    "GetAllPatientsAsync");

                return "You are not authorized to use this tool.";
            }

            var patients = await _patientService.GetAllPatientsAsync();

            if (patients.Count == 0)
                return "No patients found.";

            return string.Join(
                Environment.NewLine,
                patients.Select(p =>
                    $"PatientId: {p.PatientId}, Name: {p.Name}, Age: {p.Age}"));
        }

        [KernelFunction]
        [Description("Searches for all patients matching the patient name.")]
        public async Task<string> SearchPatientsAsync(
                                 [Description("The patient name to search for.")]
                                  string patientName)
        {
            var patients = await _patientService.SearchPatientsAsync(patientName);

            if (patients.Count == 0)
                return $"No patients found with name '{patientName}'.";

            return string.Join(
                Environment.NewLine,
                patients.Select(p =>
                    $"PatientId: {p.PatientId}, Name: {p.Name}, Age: {p.Age}"));
        }

        [KernelFunction]
        [Description("Creates a new patient in the healthcare database.")]
        public async Task<string> CreatePatientAsync([Description("The name of the new patient.")] string name,
                                                    [Description("The age of the new patient.")] int age)
        {
            var patient = await _patientService.CreatePatientAsync(name, age);

            return $"Patient created successfully. PatientId: {patient.PatientId}, Name: {patient.Name}, Age: {patient.Age}.";
        }

        [KernelFunction]
        [Description("Update a exsiting patient in the healthcare database.")]
        public async Task<string> UpdatePatientAsync([Description("The name of the patient to update.")] string patientName,
                                                    [Description("The new age of the patient.")] int newAge)
        {
            var patient = await _patientService.UpdatePatientAsync(patientName, newAge);

            if (patient == null)
                return $"Patient '{patientName}' not found.";

            return $"Patient updated successfully. PatientId: {patient.PatientId}, Name: {patient.Name}, Age: {patient.Age}.";
        }

        [KernelFunction]
        [Description("Creates an appointment for a patient.")]
        public async Task<string> CreateAppointmentAsync(
                                    [Description("The patient ID.")]
                                    string patientId,
                                    [Description("The appointment date and time in yyyy-MM-dd HH:mm format.")]
                                    string appointmentDate,
                                    [Description("The name of the doctor for the appointment.")]
                                    string doctorName)
        {
            if (!DateTime.TryParse(appointmentDate, out var parsedDate))
                return "Invalid appointment date. Please provide date and time.";

            var appointment = await _appointmentService.CreateAppointmentAsync(patientId, parsedDate.ToString("yyyy-MM-dd HH:mm"), doctorName);

            return $"Appointment created successfully. AppointmentId: {appointment.Id}, PatientId: {appointment.PatientId}," +
                $" Doctor: {appointment.DoctorName}, Date: {appointment.AppointmentDate}, Status: {appointment.Status}.";
        }

        //Multi-step Workflow Tool

        [KernelFunction]
        [Description("Use this function when the user wants to create or book an appointment by giving a patient NAME. First find the patient by name, then create the appointment for that patient.")]
        public async Task<string> CreateAppointmentForPatientAsync(
                                [Description("The exact name of the patient. Example: Kruhnsa")]
                                string patientName,

                                [Description("Appointment date and time in EXACT format yyyy-MM-dd HH:mm. Example: 2026-11-10 10:00")]
                                string appointmentDate,

                                [Description("The doctor's name. Example: Dr. Smith")]
                                string doctorName)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "Tool execution started. Tool={ToolName}, UserId={UserId}",
                "CreateAppointmentForPatientAsync",
                _requestContext.UserId);

            ////if (!DateTime.TryParse(appointmentDate, out var parsedDate))
            ////    return "Invalid appointment date. Please provide date and time.";

            if (!DateTime.TryParse(appointmentDate, out var parsedDate))
            {
                stopwatch.Stop();

                _logger.LogWarning(
                    "Tool execution failed. Tool={ToolName}, UserId={UserId}, DurationMs={DurationMs}",
                    "CreateAppointmentForPatientAsync",
                    _requestContext.UserId,
                    stopwatch.ElapsedMilliseconds);

                return "Invalid appointment date/time.";
            }

            ////try
            ////{
            ////    var appointment = await _appointmentService.CreateAppointmentForPatientAsync
            ////                                                (patientName, parsedDate.ToString("yyyy-MM-dd HH:mm"), doctorName);
            ////    if(appointment == null)
            ////    {
            ////        return $"Patient '{patientName}' not found.";
            ////    }

            ////    return System.Text.Json.JsonSerializer.Serialize(appointment);
            ////}
            ////catch (InvalidOperationException ex)
            ////{
            ////    return ex.Message;
            ////}

            try
            {
                var appointment =
                    await _appointmentService.CreateAppointmentForPatientAsync(
                        patientName, parsedDate.ToString("yyyy-MM-dd HH:mm"), doctorName);

                stopwatch.Stop();

                _logger.LogInformation(
                    "Tool execution completed. Tool={ToolName}, UserId={UserId}, DurationMs={DurationMs}",
                    "CreateAppointmentForPatientAsync",
                    _requestContext.UserId,
                    stopwatch.ElapsedMilliseconds);

                if (appointment == null)
                    return "Appointment could not be created.";

                return JsonSerializer.Serialize(appointment);
            }
            catch (InvalidOperationException ex)
            {
                stopwatch.Stop();

                _logger.LogWarning(
                    "Tool execution failed. Tool={ToolName}, UserId={UserId}, DurationMs={DurationMs}, Error={Error}",
                    "CreateAppointmentForPatientAsync",
                    _requestContext.UserId,
                    stopwatch.ElapsedMilliseconds,
                    ex.Message);

                return ex.Message;
            }
        }

    }
}
