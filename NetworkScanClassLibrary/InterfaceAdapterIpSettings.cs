using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkScanClassLibrary
{
    public class InterfaceAdapterIpSettings
    {
        public string IpAddress { get; set; }
        public string Subnet { get; set; }
        public InterfaceAdapterIpSettings()
        {
            IpAddress = "127.0.0.1";
            Subnet = "255.0.0.0";
        }
        public InterfaceAdapterIpSettings(string ipAddress, string subnet)
        {
            IpAddress = ipAddress;
            Subnet = subnet;
        }

        public override bool Equals(object other)
        {
            var toCompareWith = other as InterfaceAdapterIpSettings;
            if (toCompareWith == null)
                return false;
            return this.IpAddress == toCompareWith.IpAddress &&
                this.Subnet == toCompareWith.Subnet;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
