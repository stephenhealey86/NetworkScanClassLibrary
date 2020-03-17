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
    public class NetworkPing
    {
        private Ping ping = new Ping();
        private bool cancelFlag = false;
        public delegate void Del(string ipAddress);
        public delegate Task DelAsync(string ipAddress);
        public Del ScanNetworkCurrentIpAddressDelegate;
        public Del ScanNetworkFoundDelegate;
        public DelAsync ScanNetworkFoundAsyncDelegate;

        /// <summary>
        /// Returns the average response time of a ping in milliseconds as a string
        /// </summary>
        /// <param name="ipAddress">IP Addrss to scan</param>
        /// <returns></returns>
        public async Task<string> ScanAddress(string ipAddress, int timeout = 500, int numberOfPings = 8)
        {
            if (IsValidateIP(ipAddress))
            {
                var pingReplyList = new List<PingReply>();
                for (int i = 0; i < numberOfPings; i++)
                {
                    var respone = await ping.SendPingAsync(ipAddress, timeout);
                    if (respone.Status == IPStatus.Success)
                    {
                        pingReplyList.Add(respone);
                    }
                }
                if (pingReplyList.Count > 0)
                {
                    long totalTime = 0;
                    long maxTime = 0;
                    foreach (var res in pingReplyList)
                    {
                        totalTime += res.RoundtripTime;
                        maxTime = res.RoundtripTime > maxTime ? res.RoundtripTime : maxTime;
                    }
                    var averageTime = totalTime / pingReplyList.Count;
                    return averageTime.ToString();
                }
            }
            return "Failed";
        }

        public async Task<Tuple<string, string>> ScanAddressMaxTime(string ipAddress, int timeout = 500, int numberOfPings = 8)
        {
            if (IsValidateIP(ipAddress))
            {
                var pingReplyList = new List<PingReply>();
                for (int i = 0; i < numberOfPings; i++)
                {
                    var respone = await ping.SendPingAsync(ipAddress, timeout);
                    if (respone.Status == IPStatus.Success)
                    {
                        pingReplyList.Add(respone);
                    }
                }
                if (pingReplyList.Count > 0)
                {
                    long totalTime = 0;
                    long maxTime = 0;
                    foreach (var res in pingReplyList)
                    {
                        totalTime += res.RoundtripTime;
                        maxTime = res.RoundtripTime > maxTime ? res.RoundtripTime : maxTime;
                    }
                    var averageTime = totalTime / pingReplyList.Count;
                    return new Tuple<string, string>(averageTime.ToString(), maxTime.ToString());
                }
            }
            return new Tuple<string, string>("Failed", null);
        }

        /// <summary>
        /// Pings each address within the network range provided
        /// </summary>
        /// <param name="startAddress">IP address to start range</param>
        /// <param name="endAddress">IP address to end range</param>
        /// <param name="subnet">Network address</param>
        /// <returns>Returns a <see cref="List{String}"/> of address that replied</returns>
        public async Task<List<string>> ScanNetwork(string startAddress, string endAddress, string subnet)
        {
            cancelFlag = false;
            var listToReturn = new List<string>();

            if (IsValidateIP(startAddress) && IsValidateIP(endAddress) && IsValidateIP(subnet))
            {
                if (IsSameNetwork(startAddress, endAddress, subnet))
                {
                    var ipAddressess = GetRangeOfAddresses(startAddress, endAddress);
                    foreach (var address in ipAddressess)
                    {
                        if (!cancelFlag)
                        {
                            ScanNetworkCurrentIpAddressDelegate?.Invoke(address);

                            if (await ScanAddress(address, 200, 2) != "Failed")
                            {
                                listToReturn.Add(address);
                                ScanNetworkFoundDelegate?.Invoke(address);
                                if (ScanNetworkFoundAsyncDelegate != null)
                                {
                                    await ScanNetworkFoundAsyncDelegate(address);
                                }
                            }
                        }
                    }
                }
            }
            return listToReturn;
        }

        /// <summary>
        /// Pings each address in the list multiple times and stores the average response time
        /// </summary>
        /// <param name="listOfAddressToScan"><see cref="List{String}"/> of address to scan</param>
        /// <returns><see cref="List{int}"/> if any address is invalid it will return an empty list, response time is set to -1 if timeour occurs</returns>
        public async Task<List<int>> ScanAddressesForAverageResponseTime(List<string> listOfAddressToScan)
        {
            cancelFlag = false;
            var listToReturn = new List<int>();

            // Return empty list if any address is not valid
            foreach (var address in listOfAddressToScan)
            {
                if (!IsValidateIP(address))
                {
                    return listToReturn;
                }
            }

            foreach (var address in listOfAddressToScan)
            {
                if (!cancelFlag)
                {
                    var result = await ScanAddress(address);
                    if (result != "Failed")
                    {
                        listToReturn.Add(int.Parse(result));
                    }
                    else
                    {
                        listToReturn.Add(-1);
                    }
                }
            }

            return listToReturn;
        }

        /// <summary>
        /// Checks if the string is a vaild IP address format
        /// </summary>
        /// <param name="Address">IP address to check</param>
        /// <returns></returns>
        public bool IsValidateIP(string Address)
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
        public bool IsSameNetwork(string startAddress, string endAddress, string subnet)
        {
            byte[] startAddressByteArray = new byte[4];
            byte[] endAddressArray = new byte[4];
            byte[] subnetByteArray = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                startAddressByteArray[i] = (byte) int.Parse(startAddress.Split('.')[i]);
                endAddressArray[i] = (byte) int.Parse(endAddress.Split('.')[i]);
                subnetByteArray[i] = (byte) int.Parse(subnet.Split('.')[i]);

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
        public List<string> GetRangeOfAddresses(string startAddress, string endAddress)
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

        public void CancelAllScanning()
        {
            cancelFlag = true;
        }

        public string GetFirstIpAddressInNetwork()
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

        public string GetLastIpAddressInNetwork()
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

        public string GetSubnetAddressOfNetwork()
        {
            return GetLocalIpAddressAndSubnet().Subnet;
        }

        public bool GetInterfaceAdapterConnectedStatus()
        {
            return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
        }

        /// <summary>
        /// Checks if any interface of type ethernet or wifi is connected and returns its IP address
        /// </summary>
        /// <returns><see cref="string"/>IP address as string</returns>
        public InterfaceAdapterIpSettings GetLocalIpAddressAndSubnet()
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

        private bool InterfaceTypeIsEthernetOrWireless(NetworkInterface type)
        {
            return (type.NetworkInterfaceType == NetworkInterfaceType.Ethernet) || (type.NetworkInterfaceType == NetworkInterfaceType.Wireless80211);
        }

        private bool InterfaceIsOperationalAndHadGatewayAssigned(NetworkInterface inter)
        {
            return (inter.OperationalStatus == OperationalStatus.Up) && (inter.GetIPProperties().GatewayAddresses.Count > 0);
        }
    }
}
