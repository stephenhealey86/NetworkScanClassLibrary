using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetworkScanClassLibrary;
using static NetworkScanClassLibrary.NetworkSettings;

namespace NetworkScanUnitTest
{
    [TestClass]
    public class NetworkSettingsTest
    {
        #region IsValidateIpUnitTest
        [TestMethod]
        public void IsValidateIPShouldReturnTrue()
        {
            // Arrange
            var validAddress = "192.168.1.1";
            // Act & Assert
            Assert.IsTrue(IsValidateIP(validAddress));
        }

        [TestMethod]
        public void IsValidateIPShouldReturnFalse()
        {
            // Arrange
            var inValidAddress = "192.168.1.1444";
            // Act & Assert
            Assert.IsFalse(IsValidateIP(inValidAddress));
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
            Assert.IsTrue(IsSameNetwork(startAddress, endAddress, subnet));
        }

        [TestMethod]
        public void IsSameNetworkShouldReturnFalse()
        {
            // Arrange
            var startAddress = "192.168.11.200";
            var endAddress = "192.168.14.220";
            var subnet = "255.255.255.0";
            // Act & Assert
            Assert.IsFalse(IsSameNetwork(startAddress, endAddress, subnet));
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
            Assert.IsTrue(GetRangeOfAddresses(startAddress, endAddress).Count > 0);
        }

        [TestMethod]
        public void GetRangeOfAddressesShouldReturnEmptyList()
        {
            // Arrange
            var startAddress = "192.168.1.220";
            var endAddress = "192.168.1.200";
            // Act & Assert
            Assert.IsTrue(GetRangeOfAddresses(startAddress, endAddress).Count == 0);
        }
        #endregion

        #region GetLocalIpAddressAndSubnetUnitTest
        [TestMethod]
        public void GetLocalIpAddressAndSubnetReturnIpAddressAndSubnet()
        {
            // Arrange & Act
            var actualIpSettings = new InterfaceAdapterIpSettings()
            {
                IpAddress = "192.168.1.12",
                Subnet = "255.255.255.0"
            };
            var ipSettings = GetLocalIpAddressAndSubnet();
            // Assert
            Assert.AreEqual(actualIpSettings, ipSettings);
        }
        #endregion

        #region GetInterfaceAdapterConnectedStatus
        [TestMethod]
        public void GetInterfaceAdapterConnectedStatusShouldReturnTrue()
        {
            // Act & Assert
            Assert.IsTrue(GetInterfaceAdapterConnectedStatus());
        }
        #endregion

        #region GetFirstIpAddressInNetworkUnitTest
        [TestMethod]
        public void GetFirstIpAddressInNetworkShouldReturnFirstAddress()
        {
            // Arrange
            var firstIpAddress = "192.168.1.1";
            // Act
            var result = GetFirstIpAddressInNetwork();
            // Assert
            Assert.AreEqual(firstIpAddress, result);
        }
        #endregion

        #region GetLastIpAddressInNetworkUnitTest
        [TestMethod]
        public void GetLastIpAddressInNetworkShouldReturnLastIpAddress()
        {
            // Arrange
            var ipAddress = "192.168.1.254";
            // Act
            var result = GetLastIpAddressInNetwork();
            // Assert
            Assert.AreEqual(ipAddress, result);
        }
        #endregion
    }
}
