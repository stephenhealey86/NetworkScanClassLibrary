using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NetworkScanClassLibrary
{
    public static class NetworkSettings
    {
        /// <summary>
        /// Checks if the string is a vaild IP address format
        /// </summary>
        /// <param name="Address">IP address to check</param>
        /// <returns></returns>
        public static bool IsValidateIP(string Address)
        {
            //Match pattern for IP address    
            string Pattern = @"^([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])(\.([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])){3}$";
            //Regular Expression object    
            Regex check = new Regex(Pattern);

            //check to make sure an ip address was provided    
            if (string.IsNullOrEmpty(Address))
                //returns false if IP is not provided    
                return false;
            else
                //Matching the pattern    
                return check.IsMatch(Address, 0);
        }

        /// <summary>
        /// Checks if two Ip address are in the same subnet
        /// </summary>
        /// <param name="startAddress">First IP address</param>
        /// <param name="endAddress">Second IP address</param>
        /// <param name="subnet">Subnet to check against</param>
        /// <returns></returns>
        public static bool IsSameNetwork(string startAddress, string endAddress, string subnet)
        {
            byte[] startAddressByteArray = new byte[4];
            byte[] endAddressArray = new byte[4];
            byte[] subnetByteArray = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                startAddressByteArray[i] = (byte)int.Parse(startAddress.Split('.')[i]);
                endAddressArray[i] = (byte)int.Parse(endAddress.Split('.')[i]);
                subnetByteArray[i] = (byte)int.Parse(subnet.Split('.')[i]);

                startAddressByteArray[i] = (byte)(startAddressByteArray[i] & subnetByteArray[i]);
                endAddressArray[i] = (byte)(endAddressArray[i] & subnetByteArray[i]);

                if (startAddressByteArray[i] != endAddressArray[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns a list of IP address including and between the given start and end addresses
        /// </summary>
        /// <param name="startAddress">IP address to start range</param>
        /// <param name="endAddress">IP address to end range</param>
        /// <returns><see cref="List{String}<"/></returns>
        public static List<string> GetRangeOfAddresses(string startAddress, string endAddress)
        {
            var listToReturn = new List<string>();

            if (IsValidateIP(startAddress) && IsValidateIP(endAddress))
            {
                byte[] startAddressByteArray = new byte[4];
                byte[] endAddressArray = new byte[4];

                for (int i = 0; i < 4; i++)
                {
                    startAddressByteArray[i] = (byte)int.Parse(startAddress.Split('.')[i]);
                    endAddressArray[i] = (byte)int.Parse(endAddress.Split('.')[i]);
                }

                Array.Reverse(startAddressByteArray);
                var start = BitConverter.ToUInt32(startAddressByteArray, 0);
                Array.Reverse(endAddressArray);
                var end = BitConverter.ToUInt32(endAddressArray, 0);
                if (start <= end)
                {
                    for (uint i = start; i <= end; i++)
                    {
                        byte[] bytes = BitConverter.GetBytes(i);
                        listToReturn.Add(new IPAddress(new[] { bytes[3], bytes[2], bytes[1], bytes[0] }).ToString());
                    }
                    return listToReturn;
                }
            }

            return listToReturn;
        }

        public static string GetFirstIpAddressInNetwork()
        {
            var ipSettings = GetLocalIpAddressAndSubnet();
            if (ipSettings.IpAddress != "127.0.0.1")
            {
                var byteIpaddressToReturn = new byte[4];
                var byteActualIpaddress = new byte[4];
                var byteActualSubnet = new byte[4];

                for (int i = 0; i < 4; i++)
                {
                    byteActualIpaddress[i] = (byte)int.Parse(ipSettings.IpAddress.Split('.')[i]);
                    byteActualSubnet[i] = (byte)int.Parse(ipSettings.Subnet.Split('.')[i]);
                    byteIpaddressToReturn[i] = (byte)(byteActualIpaddress[i] & byteActualSubnet[i]);
                }
                byteIpaddressToReturn[3]++;
                return new IPAddress(new[] { byteIpaddressToReturn[0], byteIpaddressToReturn[1], byteIpaddressToReturn[2], byteIpaddressToReturn[3] }).ToString();
            }
            return ipSettings.IpAddress;
        }

        public static string GetLastIpAddressInNetwork()
        {
            var ipSettings = GetLocalIpAddressAndSubnet();
            if (ipSettings.IpAddress != "127.0.0.1")
            {
                var byteIpaddressToReturn = new byte[4];
                var byteActualIpaddress = new byte[4];
                var byteActualSubnet = new byte[4];

                for (int i = 0; i < 4; i++)
                {
                    byteActualIpaddress[i] = (byte)int.Parse(ipSettings.IpAddress.Split('.')[i]);
                    byteActualSubnet[i] = (byte)int.Parse(ipSettings.Subnet.Split('.')[i]);
                    byteIpaddressToReturn[i] = (byte)(byteActualIpaddress[i] & byteActualSubnet[i]);
                    if (byteIpaddressToReturn[i] == 0 && byteActualSubnet[i] == 0)
                    {
                        byteIpaddressToReturn[i] = 255;
                    }
                }
                byteIpaddressToReturn[3]--;
                return new IPAddress(new[] { byteIpaddressToReturn[0], byteIpaddressToReturn[1], byteIpaddressToReturn[2], byteIpaddressToReturn[3] }).ToString();
            }
            return "127.255.255.254";
        }

        public static string GetSubnetAddressOfNetwork()
        {
            return GetLocalIpAddressAndSubnet().Subnet;
        }

        public static bool GetInterfaceAdapterConnectedStatus()
        {
            return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
        }

        /// <summary>
        /// Checks if any interface of type ethernet or wifi is connected and returns its IP address
        /// </summary>
        /// <returns><see cref="string"/>IP address as string</returns>
        public static InterfaceAdapterIpSettings GetLocalIpAddressAndSubnet()
        {
            if (GetInterfaceAdapterConnectedStatus())
            {
                foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (InterfaceTypeIsEthernetOrWireless(item) && InterfaceIsOperationalAndHadGatewayAssigned(item))
                    {
                        foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                return new InterfaceAdapterIpSettings()
                                {
                                    IpAddress = ip.Address.ToString(),
                                    Subnet = ip.IPv4Mask.ToString()
                                };
                            }
                        }
                    }
                }
            }

            return new InterfaceAdapterIpSettings();
        }

        private static bool InterfaceTypeIsEthernetOrWireless(NetworkInterface type)
        {
            return (type.NetworkInterfaceType == NetworkInterfaceType.Ethernet) || (type.NetworkInterfaceType == NetworkInterfaceType.Wireless80211);
        }

        private static bool InterfaceIsOperationalAndHadGatewayAssigned(NetworkInterface inter)
        {
            return (inter.OperationalStatus == OperationalStatus.Up) && (inter.GetIPProperties().GatewayAddresses.Count > 0);
        }
    }
}
