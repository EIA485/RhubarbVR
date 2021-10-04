using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;

using Steam;
namespace RhubarbEngine.World.Net
{
	public class SteamPeer : Peer
	{
		public SteamNetModule netModule;
        public Networking.ISteamNetworkingSockets client;
        public bool IsClientConnection;
        public uint clientConnected;
		public SteamPeer(SteamNetModule _netModule,uint client)
		{
			netModule = _netModule;
            clientConnected = client;
        }

        public SteamPeer(SteamNetModule _netModule, Networking.SteamNetworkingIPAddr address)
        {
            client = Networking.SteamAPI_SteamNetworkingSockets_v009();
            netModule = _netModule;
            clientConnected = client.ConnectByIPAddress(ref address,0,new Networking.SteamNetworkingConfigValue_t { });
            IsClientConnection = true;
            Send(netModule.ConectionReqweset(), ReliabilityLevel.Reliable);
        }
        public override void Send(byte[] val, ReliabilityLevel reliableOrdered)
		{
            long outmsg = 0; 
            if (IsClientConnection)
            {
                switch (reliableOrdered)
                {
                    case ReliabilityLevel.Unreliable:
                        client.SendMessageToConnection(clientConnected, val,(uint)val.Length, Networking.k_nSteamNetworkingSend_Unreliable,ref outmsg);
                        break;
                    case ReliabilityLevel.LatestOnly:
                        client.SendMessageToConnection(clientConnected, val, (uint)val.Length, Networking.k_nSteamNetworkingSend_NoNagle, ref outmsg);
                        break;
                    case ReliabilityLevel.Reliable:
                        client.SendMessageToConnection(clientConnected, val, (uint)val.Length, Networking.k_nSteamNetworkingSend_Reliable, ref outmsg);
                        break;
                    default:
                        client.SendMessageToConnection(clientConnected, val, (uint)val.Length, Networking.k_nSteamNetworkingSend_NoDelay, ref outmsg);
                        break;
                }
            }
            else
            {
                switch (reliableOrdered)
                {
                    case ReliabilityLevel.Unreliable:
                        netModule.server.SendMessageToConnection(clientConnected, val, (uint)val.Length, Networking.k_nSteamNetworkingSend_Unreliable, ref outmsg);
                        break;
                    case ReliabilityLevel.LatestOnly:
                        netModule.server.SendMessageToConnection(clientConnected, val, (uint)val.Length, Networking.k_nSteamNetworkingSend_NoNagle, ref outmsg);
                        break;
                    case ReliabilityLevel.Reliable:
                        netModule.server.SendMessageToConnection(clientConnected, val, (uint)val.Length, Networking.k_nSteamNetworkingSend_Reliable, ref outmsg);
                        break;
                    default:
                        netModule.server.SendMessageToConnection(clientConnected, val, (uint)val.Length, Networking.k_nSteamNetworkingSend_NoDelay, ref outmsg);
                        break;
                }
            }
		}
	}


    public class SteamNetModule : NetModule
    {
        public Networking.ISteamNetworkingSockets server;
        public uint pollGroup;
        public override void Connect(string token)
		{
            var address = new Networking.SteamNetworkingIPAddr();
            var colonIndex = token.IndexOf(':');
            var host = token.Substring(0, colonIndex);
            var port = token.Substring(colonIndex + 1);
            address.m_port = ushort.Parse(port);
            rhuPeers.Add(new SteamPeer(this, address));
            pollGroup = server.CreatePollGroup();
        }


        public override IReadOnlyList<Peer> Peers { get { return rhuPeers; } }

		public List<SteamPeer> rhuPeers = new();

		public SteamNetModule(World world) : base(world)
		{
			Console.WriteLine("Starting net");
            var address = new Networking.SteamNetworkingIPAddr
            {
                m_port = 5271
            };
            server = Networking.SteamAPI_SteamNetworkingSockets_v009();
            server.CreateListenSocketIP(ref address, 0, new Networking.SteamNetworkingConfigValue_t { });
            
        }

        public SteamPeer GetPearFromClientID(uint id)
        {
            foreach (var item in rhuPeers)
            {
                if (!item.IsClientConnection)
                {
                    if(item.clientConnected == id)
                    {
                        return item;
                    }
                }
            }
            return null;
        }


        public unsafe override void Netupdate()
        {
            base.Netupdate();
            server.RunCallbacks();
            var netMessagesCount = server.ReceiveMessagesOnPollGroup(pollGroup, out var netMessages,100);

            if (netMessagesCount > 0)
            {
                for (var i = 0; i < netMessagesCount; i++)
                {
                    ref var netMessage = ref netMessages[i];
                    var peer = GetPearFromClientID(netMessage.m_conn);
                    if(peer is null)
                    {
                        peer = new SteamPeer(this, netMessage.m_conn);
                        rhuPeers.Add(peer);
                    }
                    _world.NetworkReceiveEvent(new Span<byte>(netMessage.m_pData.ToPointer(), netMessage.m_cbSize).ToArray(), peer);
                }
            }
        }

        public override unsafe void SendData(DataNodeGroup node, NetData item)
		{
            var data = node.GetByteArray();
            SendToAll(data, item.reliabilityLevel);
		}

        public void SendToAll(byte[] data, ReliabilityLevel reliableOrdered)
        {
            foreach (var item in rhuPeers)
            {
                item.Send(data, reliableOrdered);
            }
        }

        public byte[] ConectionReqweset()
        {
            var req = new DataNodeGroup();
            return req.GetByteArray();
        }

        public override void Dispose()
        {
        }

    }
}
