namespace EPAD_Logic.Configuration
{
    public interface IThirdPartyIntegrationConfigurationService
    {
        public string GetConnectionString { get; }
        public string GetConnectionStringHRPRO7 { get; }
        public string GetConnectionStringHIK { get; }

    }
}
