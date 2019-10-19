using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
using System;
using System.Threading.Tasks;

[assembly: FabricTransportServiceRemotingProvider(RemotingListenerVersion = RemotingListenerVersion.V2, RemotingClientVersion = RemotingClientVersion.V2)]

namespace GuyRemoteServices
{
    public interface IGuyStatefulServiceCoreInterface : IService 
    {
        Task SpawnGuy(string name);
        Task<string> RetrieveGuys();
        
    }
}
