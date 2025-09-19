namespace EPAD_Common.Types
{
    public class ThirdPartyIntegrationConfiguration
    {
        public const string ThirdPartyIntegration = nameof(ThirdPartyIntegration);

        public string ConnectionString { get; set; }
        public string ConnectionStringHRPRO7 { get; set; }
        public string ConnectionStringHIK { get; set; }

    }
}
