using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetworkScanClassLibrary;

namespace NetworkScanUnitTest
{
    [TestClass]
    public class NetworkPingTest
    {
        private NetworkPing ping = new NetworkPing();

        #region ScanAddressUnitTests
        [TestMethod]
        [Timeout(5000)]
        public async Task ScanAddressShouldReturnFailed()
        {
            // Arrange
            var ipAddress = "200.168.1.1";
            // Act
            var response = await ping.ScanAddress(ipAddress);
            // Assert
            Assert.AreEqual(response, "Failed");
        }

        [TestMethod]
        public async Task ScanAddressShouldFailOnInvalidIp()
        {
            // Arrange
            var ipAddress = "192.168";
            // Act
            var response = await ping.ScanAddress(ipAddress);
            // Assert
            Assert.AreEqual(response, "Failed");
        }

        [TestMethod]
        [Timeout(4000)]
        public async Task ScanAddressShouldReturnTime()
        {
            // Arrange
            var ipAddress = "8.8.8.8";
            // Act
            var response = await ping.ScanAddress(ipAddress);
            // Assert
            Assert.IsTrue(int.Parse(response) >= 0);
        }
        #endregion

        #region ScanNetworkUnitTests
        [TestMethod]
        public async Task ScanNetworkShouldReturnEmptyList()
        {
            // Arrange
            var list = new List<string>();
            var startAddress = "192.168.1.200";
            var endAddress = "192.168.1.205";
            var subnet = "255.255.255.0";
            // Act
            list = await ping.ScanNetwork(startAddress, endAddress, subnet);
            // Assert
            Assert.IsTrue(list.Count == 0);
        }

        [TestMethod]
        public async Task ScanNetworkShouldReturnAtLeastOne()
        {
            // Arrange
            var list = new List<string>();
            var startAddress = "192.168.1.1";
            var endAddress = "192.168.1.50";
            var subnet = "255.255.255.0";
            // Act
            list = await ping.ScanNetwork(startAddress, endAddress, subnet);
            // Assert
            Assert.IsTrue(list.Count > 0);
        }
        
        [TestMethod]
        public async Task ScanNetworkDelegateShouldPassBackIpAddresses()
        {
            // Arrange
            var list = new List<string>();
            var startAddress = "192.168.1.1";
            var endAddress = "192.168.1.50";
            var subnet = "255.255.255.0";
            ping.ScanNetworkFoundDelegate += (value) =>
            {
                list.Add(value);
            };
            // Act
            await ping.ScanNetwork(startAddress, endAddress, subnet);
            // Assert
            Assert.IsTrue(list.Count > 0);
        }
        #endregion

        #region ScanAddressesForAverageResponseTimeUnitTests
        [TestMethod]
        public async Task ScanAddressesForAverageResponseTimeShouldReturnAtLeastOne()
        {
            // Arrange
            var listOfAddressToScan = new List<string>()
            {
                "192.168.1.1",
                "192.168.1.20",
                "192.168.1.30"
            };
            // Act
            var listOfResponseTimes = await ping.ScanAddressesForAverageResponseTime(listOfAddressToScan);
            // Assert
            Assert.IsTrue(listOfResponseTimes.Count > 0);
        }

        [TestMethod]
        public async Task ScanAddressesForAverageResponseTimeShouldReturnListOfTimeouts()
        {
            // Arrange
            var listOfAddressToScan = new List<string>()
            {
                "192.168.1.200",
                "192.168.1.210",
                "192.168.1.220"
            };
            // Act
            var listOfResponseTimes = await ping.ScanAddressesForAverageResponseTime(listOfAddressToScan);
            // Assert
            Assert.IsTrue(listOfResponseTimes.Count > 0);
            foreach (var res in listOfResponseTimes)
            {
                Assert.AreEqual(res, -1);
            }
        }

        [TestMethod]
        public async Task ScanAddressesForAverageResponseTimeShouldReturnEmptyListDueToInvalidIPAddress()
        {
            // Arrange
            var listOfAddressToScan = new List<string>()
            {
                "192.168.1.256",
                "192.168.1.1"
            };
            // Act
            var listOfResponseTimes = await ping.ScanAddressesForAverageResponseTime(listOfAddressToScan);
            // Assert
            Assert.IsTrue(listOfResponseTimes.Count == 0);
        }
        #endregion

        #region IsValidateIpUnitTest
        [TestMethod]
        public void IsValidateIPShouldReturnTrue()
        {
            // Arrange
            var validAddress = "192.168.1.1";
            // Act & Assert
            Assert.IsTrue(ping.IsValidateIP(validAddress));
        }

        [TestMethod]
        public void IsValidateIPShouldReturnFalse()
        {
            // Arrange
            var inValidAddress = "192.168.1.1444";
            // Act & Assert
            Assert.IsFalse(ping.IsValidateIP(inValidAddress));
        }
        #endregion

        #region IsSameNetworkUnitTest
        [TestMethod]
        public void IsSameNetworkShouldReturnTrue()
        {
            // Arrange
            var startAddress = "192.168.14.200";
            var endAddress = "192.168.14.220";
            var subnet = "255.255.255.0";
            // Act & Assert
            Assert.IsTrue(ping.IsSameNetwork(startAddress, endAddress, subnet));
        }

        [TestMethod]
        public void IsSameNetworkShouldReturnFalse()
        {
            // Arrange
            var startAddress = "192.168.11.200";
            var endAddress = "192.168.14.220";
            var subnet = "255.255.255.0";
            // Act & Assert
            Assert.IsFalse(ping.IsSameNetwork(startAddress, endAddress, subnet));
        }
        #endregion

        #region GetRangeOfAddressesUnitTest
        [TestMethod]
        public void GetRangeOfAddressesShouldReturnList()
        {
            // Arrange
            var startAddress = "192.168.0.200";
            var endAddress = "192.168.1.255";
            // Act & Assert
            Assert.IsTrue(ping.GetRangeOfAddresses(startAddress, endAddress).Count > 0);
        }

        [TestMethod]
        public void GetRangeOfAddressesShouldReturnEmptyList()
        {
            // Arrange
            var startAddress = "192.168.1.220";
            var endAddress = "192.168.1.200";
            // Act & Assert
            Assert.IsTrue(ping.GetRangeOfAddresses(startAddress, endAddress).Count == 0);
        }
        #endregion

        #region GetLocalIpAddressAndSubnetUnitTest
        [TestMethod]
        public void GetLocalIpAddressAndSubnetReturnIpAddressAndSubnet()
        {
            // Arrange & Act
            var actualIpSettings = new InterfaceAdapterIpSettings()
            {
                IpAddress = "10.0.169.53",
                Subnet = "255.255.0.0"
            };
            var ipSettings = ping.GetLocalIpAddressAndSubnet();
            // Assert
            Assert.AreEqual(actualIpSettings, ipSettings);
        }
        #endregion

        #region GetInterfaceAdapterConnectedStatus
        [TestMethod]
        public void GetInterfaceAdapterConnectedStatusShouldReturnTrue()
        {
            // Act & Assert
            Assert.IsTrue(ping.GetInterfaceAdapterConnectedStatus());
        }
        #endregion

        #region GetFirstIpAddressInNetworkUnitTest
        [TestMethod]
        public void GetFirstIpAddressInNetworkShouldReturnFirstAddress()
        {
            // Arrange
            var firstIpAddress = "10.0.0.1";
            // Act
            var result = ping.GetFirstIpAddressInNetwork();
            // Assert
            Assert.AreEqual(firstIpAddress, result);
        }
        #endregion

        #region GetLastIpAddressInNetworkUnitTest
        [TestMethod]
        public void GetLastIpAddressInNetworkShouldReturnLastIpAddress()
        {
            // Arrange
            var ipAddress = "10.0.255.254";
            // Act
            var result = ping.GetLastIpAddressInNetwork();
            // Assert
            Assert.AreEqual(ipAddress, result);
        }
        #endregion
    }
}
