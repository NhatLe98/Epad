using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Common.Enums
{
    public enum DeviceType
    {
        Card, Finger, Face, CardAndFinger, All
    }

    public enum ControllerType
    {
        In, Out
    }

    public enum TimeMode
    {
        In, Out
    }

    public enum PrivilegeGroup
    { 
        Device, GroupDevice, DeviceModule
    }

    public enum GCSInOutMode
    {
        Input = 1,
        Output = 2,
    }

    public enum DeviceInOutMode
    {
        Unknown, In, Out, BreakOut, BreakIn
    }

    public enum LogType
    {
        Walker, Customer, Parking
    }
}
