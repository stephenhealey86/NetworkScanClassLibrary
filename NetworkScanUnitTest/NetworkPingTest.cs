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
        public async Task ScanAddressShouldReturnFalse()
        {
            // Arrange
            var ipAddress = "200.168.1.1";
            // Act
            var response = await ping.ScanAddress(ipAddress);
            // Assert
            Assert.IsFalse(response);
        }

        [TestMethod]
        public async Task ScanAddressShouldFailOnInvalidIp()
        {
            // Arrange
            var ipAddress = "192.168";
            // Act
            var response = await ping.ScanAddress(ipAddress);
            // Assert
            Assert.IsFalse(response);
        }

        [TestMethod]
        [Timeout(4000)]
        public async Task ScanAddressShouldReturnTrue()
        {
            // Arrange
            var ipAddress = "8.8.8.8";
            // Act
            var response = await ping.ScanAddress(ipAddress);
            // Assert
            Assert.IsTrue(response);
        }
        #endregion

        #region ScanAddressForResponseTimesUnitTests
        [TestMethod]
        [Timeout(5000)]
        public async Task ScanAddressForResponseTimesShouldReturnTimeout()
        {
            // Arrange
            var ipAddress = "200.168.1.1";
            // Act
            var response = await ping.ScanAddressForResponseTimes(ipAddress);
            // Assert
            Assert.AreEqual(response.Status, ScanResponseStatus.timeout);
        }

        [TestMethod]
        public async Task ScanAddressForResponseTimesShouldFailOnInvalidIp()
        {
            // Arrange
            var ipAddress = "192.168";
            // Act
            var response = await ping.ScanAddressForResponseTimes(ipAddress);
            // Assert
            Assert.AreEqual(response.Status, ScanResponseStatus.invalidIp);
        }

        [TestMethod]
        [Timeout(4000)]
        public async Task ScanAddressForResponseTimesShouldReturnOk()
        {
            // Arrange
            var ipAddress = "8.8.8.8";
            // Act
            var response = await ping.ScanAddressForResponseTimes(ipAddress);
            // Assert
            Assert.AreEqual(response.Status, ScanResponseStatus.ok);
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
            ping.ScanNetworkFoundDelegateAsync += async (value) =>
            {
                list.Add(value);
            };
            // Act
            await ping.ScanNetwork(startAddress, endAddress, subnet);
            // Assert
            Assert.IsTrue(list.Count > 0);
        }
        #endregion
    }
}
