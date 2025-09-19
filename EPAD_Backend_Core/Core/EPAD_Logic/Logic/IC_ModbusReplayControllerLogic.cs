using EPAD_Common;
using EPAD_Data.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Logic
{
    public class IC_ModbusReplayControllerLogic : IIC_ModbusReplayControllerLogic
    {

        ModbusTCP.Device modbusDevice;
        byte[] result;

        public bool IsConnected()
        {
            bool connected = true;
            if (modbusDevice == null || modbusDevice.connected == false)
            {
                connected = false;
            }

            return connected;
        }

        public async Task<bool> ConnectToModbusTCPDevie(string pIp, ushort pPort)
        {
            try
            {
                modbusDevice = new ModbusTCP.Device();

                // Use Task.Run to execute the connection attempt in a separate thread
                var connectTask = Task.Run(() =>
                {
                    modbusDevice.connect(pIp, pPort);
                });

                // Wait for the connection task to complete or timeout after 5 seconds
                if (await Task.WhenAny(connectTask, Task.Delay(2000)) == connectTask)
                {
                    // Connection task completed within 5 seconds
                    return modbusDevice.connected;
                }
                else
                {
                    // Connection task timed out
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public string OpenChannel(List<ChannelParam> pListChannel)
        {
            ushort ID = 5;
            var output = "";
            try
            {
                if (modbusDevice.connected == false)
                {
                    output = "NotConnected";
                }
                else
                {
                    foreach (var item in pListChannel)
                    {
                        int channelIndex = item.Index - 1;
                        ushort address = Convert.ToUInt16(channelIndex.ToString(), 16);
                        byte unit = Convert.ToByte(1);
                        modbusDevice.WriteSingleCoils(ID, unit, address, item.ChannelStatus, ref result);
                        if (result == null)
                            return null;
                        //output += $" --- channelIndex:{channelIndex} address:{address} unit:{unit} item.ChannelStatus:{item.ChannelStatus} result:{result}";
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public ControllerParam GetChannelInputStatus(ControllerParam deviceInfo)
        {
            ushort ID = 1;
            if (modbusDevice.connected)
            {
                byte unit = Convert.ToByte(1);
                ushort address = Convert.ToUInt16(0.ToString(), 16);
                UInt16 Length = Convert.ToUInt16(8);
                modbusDevice.ReadDiscreteInputs(ID, unit, 1024, Length, ref result);
                var resultArray = ConvertToBits(result);
                if (resultArray != null && resultArray.Count > 0)
                {
                    for (var i = 0; i < resultArray.Length; i++)
                    {
                        deviceInfo.ListChannel[i].ChannelStatus = Convert.ToBoolean(resultArray[i]);
                        if (deviceInfo.ListChannel[i].ChannelStatus)
                            return deviceInfo;
                    }
                }
            }
            return deviceInfo;
        }

        public ControllerParam GetChannelStatus(ControllerParam deviceInfo)
        {
            ushort ID = 1;
            if (modbusDevice.connected)
            {
                byte unit = Convert.ToByte(1);
                ushort address = Convert.ToUInt16(0.ToString(), 16);
                UInt16 Length = Convert.ToUInt16(8);
                modbusDevice.ReadCoils(ID, unit, address, Length, ref result);
                var resultArray = ConvertToBits(result);
                if (resultArray != null && resultArray.Count > 0)
                {
                    for (var i = 0; i < resultArray.Length; i++)
                    {
                        deviceInfo.ListChannel[i].ChannelStatus = Convert.ToBoolean(resultArray[i]);
                    }
                }

                return deviceInfo;
            }
            return null;
        }

        public void DisconnectModbusTCPDevice()
        {
            if (modbusDevice != null)
            {
                modbusDevice.disconnect();
            }
        }

        private BitArray ConvertToBits(byte[] data)
        {
            bool[] bits = new bool[1];
            BitArray bitArray = new BitArray(data);
            bits = new bool[bitArray.Count];
            bitArray.CopyTo(bits, 0);
            return bitArray;
        }

        public async Task<string> SetOnAndAutoOffController(List<ChannelParam> listChannel, double numberOfSeconds)
        {
            foreach (var item in listChannel)
            {
                item.ChannelStatus = true;
            }
            var result = OpenChannel(listChannel);

            var offController = await SetAutoOffController(listChannel, numberOfSeconds);
            return offController;
        }

        public async Task<string> SetOffAndAutoOnController(List<ChannelParam> listChannel, double numberOfSeconds)
        {
            foreach (var item in listChannel)
            {
                item.ChannelStatus = false;
            }
            var result = OpenChannel(listChannel);

            var offController = await SetAutoOnController(listChannel, numberOfSeconds);
            return offController;
        }

        private async Task<string> SetAutoOffController(List<ChannelParam> listChannel, double numberOfSeconds)
        {
            numberOfSeconds = (numberOfSeconds > 0 ? numberOfSeconds : 4) * 1000;
            await Task.Delay((int)numberOfSeconds);
            foreach (var item in listChannel)
            {
                item.ChannelStatus = false;
            }
            var openChannel = OpenChannel(listChannel);
            return openChannel;
        }

        private async Task<string> SetAutoOnController(List<ChannelParam> listChannel, double numberOfSeconds)
        {
            numberOfSeconds = (numberOfSeconds > 0 ? numberOfSeconds : 4) * 1000;
            await Task.Delay((int)numberOfSeconds);
            foreach (var item in listChannel)
            {
                item.ChannelStatus = true;
            }
            var openChannel = OpenChannel(listChannel);
            return openChannel;
        }
    }

    public interface IIC_ModbusReplayControllerLogic
    {
        Task<bool> ConnectToModbusTCPDevie(string pIp, ushort pPort);
        string OpenChannel(List<ChannelParam> pListChannel);
        bool IsConnected();
        void DisconnectModbusTCPDevice();
        ControllerParam GetChannelStatus(ControllerParam deviceInfo);
        ControllerParam GetChannelInputStatus(ControllerParam deviceInfo);
        Task<string> SetOnAndAutoOffController(List<ChannelParam> listChannel, double numberOfSeconds);
        Task<string> SetOffAndAutoOnController(List<ChannelParam> listChannel, double numberOfSeconds);
    }
}
