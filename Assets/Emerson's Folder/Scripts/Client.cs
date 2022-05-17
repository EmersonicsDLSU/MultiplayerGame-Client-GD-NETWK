using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class Client : MonoBehaviour
{
    // singleton instance
    public static Client instance;
    // 4096 bytes(b) or 4 megabytes(mb)
    public static int dataBufferSize = 4096;
    // server's IP; IP for localhost
    public string ip = "127.0.0.1";
    // port number should be identical to the server's port
    public int port = 26950;
    // local client's ID
    public int myId = 0;
    // reference to the TCP class 
    public TCP tcp;
    // reference to the UDP class
    public UDP udp;
    // delegate for Packets
    private delegate void PacketHandler(Packet _packet);
    // Dictionary to store the packet IDs and their corresponding packetHandler
    private static Dictionary<int, PacketHandler> packetHandlers;
    // initialize our singleton
    private void Awake()
    {
        // if null, then assign it to this class instance
        if (instance == null)
        {
            instance = this;
        }
        // else, destroy the duplicate instance
        else if (instance != this)
        {
            Debug.LogError($"Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void Start()
    {
        // create a new instance for our TCP field
        tcp = new TCP();
        // create a new instance for our UDP field
        udp = new UDP();
    }

    public void ConnectToServer()
    {
        // Initialize our Dictionary instance(packetHandlers)
        InitializeClientData();

        tcp.Connect();
    }

    public class TCP
    {
        // store the instance that we get in the server's 'ConnectCallback'
        public TcpClient socket;

        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public void Connect()
        {
            // initialize a TcpClient to our socket and set its receive/send buffer sizes
            socket = new TcpClient()
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };
            // initialize our receive buffer
            receiveBuffer = new byte[dataBufferSize];
            // pass the server IP, port, ConnectCallback, and the TcpClient reference(socket)
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }

        private void ConnectCallback(IAsyncResult _result)
        {
            socket.EndConnect(_result);
            // check if the client is connected
            if (!socket.Connected)
            {
                return;
            }
            // if connected, assign the socket's stream to our client's stream
            stream = socket.GetStream();
            // initialize our Packet instance(receivedData)
            receivedData = new Packet();
            // starts receiving data
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }

        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to server via TCP: {_ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0)
                {
                    // TODO: disconnect
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);
                // take in the boolean returned by 'HandleData()'
                receivedData.Reset(HandleData(_data));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch
            {
                // TODO: disconnect
            }
        }

        private bool HandleData(byte[] _data)
        {
            int _packetLength = 0;

            receivedData.SetBytes(_data);
            // check if the received data consist of 4 unread bytes
            if (receivedData.UnreadLength() >= 4)
            {
                // assigns the unread bytes length
                _packetLength = receivedData.ReadInt();
                if (_packetLength <= 0)
                {
                    return true;
                }
            }
            // check if there is a new complete packet that was recently received
            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
            {
                // read the packet's byte, put it into an array
                byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet _packet = new Packet(_packetBytes))
                        {
                            // reads the client ID
                            int _packetId = _packet.ReadInt();
                            // invoke our delegate(PacketHandler) from our key(client ID)
                            packetHandlers[_packetId](_packet);
                        }
                    });
                // Reset the packetLength to '0'
                _packetLength = 0;
                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }
            }
            // Reset receiveData
            if (_packetLength <= 1)
            {
                return true;
            }
            // If the packetLength is greater than 1, we do not want
            // to reset receiveData because there is still a partial packet
            return false;
        }
    }
    // adding UDP support, so that clients can communicate
    // with the server through both TCP/UDP
    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP()
        {
            endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }

        public void Connect(int _localPort)
        {
            // bind the UDP client to the local port
            socket = new UdpClient(_localPort);
            // connect the socket to the endpoint
            socket.Connect(endPoint);
            // start receiving message
            socket.BeginReceive(ReceiveCallback, null);

            // create a new packet, and immediately send it
            // purpose: to initiate the connection with the server and to open
            // up the local port so that the client can receive messages
            using (Packet _packet = new Packet())
            {
                SendData(_packet);
            }
        }

        public void SendData(Packet _packet)
        {
            try
            {
                // insert the client's ID into the packet
                // to determine who sent the message
                _packet.InsertInt(instance.myId);
                // socket should not be empty
                if (socket != null)
                {
                    socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to server via UDP: {_ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                // byteArray that stores the value returned by the socket(EndReceive)
                byte[] _data = socket.EndReceive(_result, ref endPoint);
                // continue receiving message
                socket.BeginReceive(ReceiveCallback, null);
                // checks if there is an actual packet to handle
                if (_data.Length < 4)
                {
                    // TODO: disconnect
                    return;
                }

                HandleData(_data);
            }
            catch (Exception e)
            {
                // TODO: disconnect
            }
        }

        private void HandleData(byte[] _data)
        {
            // create a Packet instance with the bytes we received
            using (Packet _packet = new Packet(_data))
            {
                // removes the first four bytes from the received bytes,
                // which represent the length of the packet
                int _packetLength = _packet.ReadInt();
                _data = _packet.ReadBytes(_packetLength);
            }

            ThreadManager.ExecuteOnMainThread(() =>
            {
                // create a Packet instance with the shortened byte array
                using (Packet _packet = new Packet(_data))
                {
                    // read the packet ID
                    int _packetId = _packet.ReadInt();
                    // invoke the method to handle our packet
                    packetHandlers[_packetId](_packet);
                }
            });
        }
    }
    // Initialize the Dictionary instance(packetHandlers)
    private void InitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            {(int) ServerPackets.welcome, ClientHandle.Welcome},
            {(int) ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer},
            {(int) ServerPackets.playerPosition, ClientHandle.PlayerPosition},
            {(int) ServerPackets.playerRotation, ClientHandle.PlayerRotation}
            //{(int) ServerPackets.udpTest, ClientHandle.UDPTest} // UDP test
        };
        Debug.Log("Initialized packets.");
    }
}
