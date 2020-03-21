using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static NetworkScanClassLibrary.NetworkSettings;

namespace NetworkScanClassLibrary
{
    public class NetworkPing : INetworkPing
    {
        private Ping ping = new Ping();
        private bool cancelFlag = false;
        public event Del ScanNetworkCurrentIpAddressDelegate;
        public event DelAsync ScanNetworkFoundDelegateAsync;

        /// <summary>
        /// Returns true if address replys
        /// </summary>
        /// <param name="ipAddress">IP Address to scan</param>
        /// <returns></returns>
        public async Task<bool> ScanAddress(string ipAddress, int timeout = 500, int numberOfPings = 8)
        {
            if (IsValidateIP(ipAddress))
            {
                for (int i = 0; i < numberOfPings; i++)
                {
                    var respone = await ping.SendPingAsync(ipAddress, timeout);
                    if (respone.Status == IPStatus.Success)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<ScanResponse> ScanAddressForResponseTimes(string ipAddress, int timeout = 500, int numberOfPings = 8)
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
                    return new ScanResponse() { IpAddress = ipAddress, AverageResponse = averageTime.ToString(), MaxResponse = maxTime.ToString(), Status = ScanResponseStatus.ok };
                }
                return new ScanResponse() { IpAddress = ipAddress, AverageResponse = "Timeout", MaxResponse = "Timeout", Status = ScanResponseStatus.timeout };
            }
            return new ScanResponse() { IpAddress = ipAddress, AverageResponse = "Invalid", MaxResponse = "Invalid", Status = ScanResponseStatus.invalidIp };
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

                            if (await ScanAddress(address, 200, 2))
                            {
                                listToReturn.Add(address);
                                if (ScanNetworkFoundDelegateAsync != null)
                                {
                                    await ScanNetworkFoundDelegateAsync(address);
                                }
                            }
                        }
                    }
                }
            }
            return listToReturn;
        }

        public void CancelAllScanning()
        {
            cancelFlag = true;
        }
    }
}
