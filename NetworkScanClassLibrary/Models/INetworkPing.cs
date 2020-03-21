using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetworkScanClassLibrary
{
    public delegate void Del(string ipAddress);
    public delegate Task DelAsync(string ipAddress);

    public interface INetworkPing
    {
        event Del ScanNetworkCurrentIpAddressDelegate;
        event DelAsync ScanNetworkFoundDelegateAsync;
        void CancelAllScanning();
        Task<bool> ScanAddress(string ipAddress, int timeout = 500, int numberOfPings = 8);
        Task<ScanResponse> ScanAddressForResponseTimes(string ipAddress, int timeout = 500, int numberOfPings = 8);
        Task<List<string>> ScanNetwork(string startAddress, string endAddress, string subnet);
    }
}