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

    private delegate void PacketHandler(Packet _packet);
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

            if (receivedData.UnreadLength() >= 4)
            {
                _packetLength = receivedData.ReadInt();
                if (_packetLength <= 0)
                {
                    return true;
                }
            }

            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
            {
                byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet _packet = new Packet(_packetBytes))
                        {
                            int _packetId = _packet.ReadInt();
                            packetHandlers[_packetId](_packet);
                        }
                    });

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

            if (_packetLength <= 1)
            {
                return true;
            }

            return false;
        }
    }

    private void InitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            {(int) ServerPackets.welcome, ClientHandle.Welcome}
        };
        Debug.Log("Initialized packets.");
    }
}
