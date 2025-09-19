using System;
using System.Linq;

namespace EPAD_Common.Utility
{

    public class WindowsServiceHelper
    {
        private string mServiceName = "";
        System.ServiceProcess.ServiceController mServiceController;
        public WindowsServiceHelper(string serviceName)
        {
            mServiceName = serviceName;
            mServiceController = new System.ServiceProcess.ServiceController();
            mServiceController.ServiceName = serviceName;

        }
        public bool CheckServiceRunning()
        {
            bool value = true;
            if (mServiceController.Status == System.ServiceProcess.ServiceControllerStatus.Stopped)
            {
                value = false;
            }
            return value;
        }
        public bool CheckServiceExists()
        {
            System.ServiceProcess.ServiceController serviceCheck = System.ServiceProcess.ServiceController.GetServices()
                .Where(x => x.ServiceName == mServiceName).FirstOrDefault();
            if (serviceCheck == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public bool StartService(ref string error)
        {
            bool success = false;
            try
            {
                if (CheckServiceRunning() == false)
                {
                    mServiceController.Start();
                    success = true;
                }

            }
            catch (Exception ex)
            {
                error = ex.ToString();
            }
            return success;
        }
        public bool StopService(ref string error)
        {
            bool success = false;
            try
            {
                if (CheckServiceRunning() == true)
                {
                    mServiceController.Stop();
                    success = true;
                }

            }
            catch (Exception ex)
            {
                error = ex.ToString();
            }
            return success;
        }
    }
}
