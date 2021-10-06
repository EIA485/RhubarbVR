using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using RhubarbEngine.World.DataStructure;

using Valve.Sockets;

namespace RhubarbEngine.World.Net
{
    public class SteamPeer : Peer
    {
        public SteamNetModule netModule;
        public Native.ISteamNetworkingSockets client;
        public bool IsClientConnection;
        public uint clientConnected;
        public SteamPeer(SteamNetModule _netModule, uint client)
        {
            netModule = _netModule;
            clientConnected = client;
        }

        public SteamPeer(SteamNetModule _netModule)
        {
            netModule = _netModule;
            IsClientConnection = true;
            Send(netModule.ConectionReqweset(), ReliabilityLevel.Reliable);
        }
        public override unsafe void Send(byte[] val, ReliabilityLevel reliableOrdered)
        {
            fixed (byte* pointv = val)
            {
                if (IsClientConnection)
                {
                    switch (reliableOrdered)
                    {
                        case ReliabilityLevel.Unreliable:
                            client.SendMessageToConnection(clientConnected, (IntPtr)pointv, (uint)val.Length, Native.k_nSteamNetworkingSend_Unreliable,IntPtr.Zero);
                            break;
                        case ReliabilityLevel.LatestOnly:
                            client.SendMessageToConnection(clientConnected, (IntPtr)pointv, (uint)val.Length, Native.k_nSteamNetworkingSend_NoNagle, IntPtr.Zero);
                            break;
                        case ReliabilityLevel.Reliable:
                            client.SendMessageToConnection(clientConnected, (IntPtr)pointv, (uint)val.Length, Native.k_nSteamNetworkingSend_Reliable, IntPtr.Zero);
                            break;
                        default:
                            client.SendMessageToConnection(clientConnected, (IntPtr)pointv, (uint)val.Length, Native.k_nSteamNetworkingSend_NoDelay, IntPtr.Zero);
                            break;
                    }
                }
                else
                {
                    switch (reliableOrdered)
                    {
                        case ReliabilityLevel.Unreliable:
                            netModule.server.SendMessageToConnection(clientConnected, (IntPtr)pointv, (uint)val.Length, Native.k_nSteamNetworkingSend_Unreliable, IntPtr.Zero);
                            break;
                        case ReliabilityLevel.LatestOnly:
                            netModule.server.SendMessageToConnection(clientConnected, (IntPtr)pointv, (uint)val.Length, Native.k_nSteamNetworkingSend_NoNagle, IntPtr.Zero);
                            break;
                        case ReliabilityLevel.Reliable:
                            netModule.server.SendMessageToConnection(clientConnected, (IntPtr)pointv, (uint)val.Length, Native.k_nSteamNetworkingSend_Reliable, IntPtr.Zero);
                            break;
                        default:
                            netModule.server.SendMessageToConnection(clientConnected, (IntPtr)pointv, (uint)val.Length, Native.k_nSteamNetworkingSend_NoDelay, IntPtr.Zero);
                            break;
                    }
                }
            }
        }
    }


    public class SteamNetModule : NetModule
    {
        public Native.ISteamNetworkingSockets server;
        
        const int MAX_MESSAGES = 20;
        public Native.ISteamNetworkingMessages[] netMessages = new Native.ISteamNetworkingMessages[MAX_MESSAGES];
        public uint pollGroup;
        public override void Connect(string token)
        {
            rhuPeers.Add(new SteamPeer(this));
            pollGroup = server.CreatePollGroup();
            
        }


        public override IReadOnlyList<Peer> Peers { get { return rhuPeers; } }

        public List<SteamPeer> rhuPeers = new();

        public SteamNetModule(World world) : base(world)
        {
            Console.WriteLine("Starting net");
            _ = Native.SteamAPI_ISteamNetworkingSockets_CreateListenSocketP2P(ref server, 5276, 0, IntPtr.Zero);
        }

        public SteamPeer GetPearFromClientID(uint id)
        {
            foreach (var item in rhuPeers)
            {
                if (!item.IsClientConnection)
                {
                    if (item.clientConnected == id)
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
            var netMessagesCount = server.ReceiveMessagesOnPollGroup(pollGroup,netMessages, MAX_MESSAGES);

            if (netMessagesCount > 0)
            {
                for (var i = 0; i < netMessagesCount; i++)
                {
                    //ref var netMessage = ref netMessages[i];
                    //var peer = GetPearFromClientID(netMessage.);
                    //if (peer is null)
                    //{
                    //    peer = new SteamPeer(this, netMessage.connection);
                    //    rhuPeers.Add(peer);
                    //}
                    //_world.NetworkReceiveEvent(new Span<byte>(netMessage..ToPointer(), netMessage.length).ToArray(), peer);
                    //netMessage.Destroy();
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