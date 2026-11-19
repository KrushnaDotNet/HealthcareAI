namespace HealthcareAI.Services
{
    public interface IAgentAuthorizationService
    {
        bool IsToolAllowed(string accessLevel, string toolName);
    }

    public class AgentAuthorizationService : IAgentAuthorizationService
    {
        public bool IsToolAllowed(string accessLevel, string toolName)
        {
            if (string.IsNullOrWhiteSpace(accessLevel))
                return false;

            if (accessLevel.Equals("Admin",
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (toolName.Equals("GetAllPatientsAsync",
                StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }
    }
}