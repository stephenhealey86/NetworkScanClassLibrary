using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkScanClassLibrary
{
    public class ScanResponse
    {
        public string IpAddress { get; set; }
        public string AverageResponse { get; set; }
        public string MaxResponse { get; set; }
        public ScanResponseStatus Status { get; set; }
    }

    public enum ScanResponseStatus
    {
        ok,
        timeout,
        invalidIp
    }
}
