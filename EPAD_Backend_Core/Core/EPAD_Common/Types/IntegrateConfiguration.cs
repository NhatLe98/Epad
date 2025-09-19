namespace EPAD_Common.Types
{
    public class IntegrateConfiguration
    {
        /// <summary>
        /// Configuration for GCS project
        /// </summary>
        public AppConfiguration GCS { get; set; }

        /// <summary>
        /// Configuration for ECMS project (ICMS web version)
        /// </summary>
        public AppConfiguration ECMS { get; set; }

        /// <summary>
        /// Configuration for EZHR
        /// </summary>
        public AppConfiguration EZHR { get; set; }

        /// <summary>
        /// Configuration for ICMS (HRPro7)
        /// </summary>
        public AppConfiguration ICMS { get; set; }

        /// <summary>
        /// Configuration for Relay controller
        /// </summary>
        public AppConfiguration RelayController { get; set; }
    }
}
