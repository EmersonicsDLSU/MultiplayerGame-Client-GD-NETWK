using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// client to respond to server
public class ClientSend : MonoBehaviour
{
    // prepares packets to be send
    private static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.tcp.SendData(_packet);
    }

    private static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.udp.SendData(_packet);
    }

    #region Packets
    // creates the packet we want to send to the server,
    // once the client receives the message
    public static void WelcomeReceived()
    {
        // create a Packet instance
        using (Packet _packet = new Packet((int) ClientPackets.welcomeReceived))
        {
            // writes the client's ID
            _packet.Write(Client.instance.myId);
            // writes the client's name
            _packet.Write(UIManager.instance.usernameField.text);
            // pass the packet
            SendTCPData(_packet);
        }
    }

    public static void PlayerMovement(bool[] _inputs)
    {
        // send the movement
        using (Packet _packet = new Packet((int) ClientPackets.playerMovement))
        {
            _packet.Write(_inputs.Length);
            foreach (bool _input in _inputs)
            {
                _packet.Write(_input);
            }
            _packet.Write(GameManager.players[Client.instance.myId].transform.rotation);
            // send the packet through UDP; we can manage to lose some data in the movement
            // and UDP is much faster; since we will send it over and over again
            SendUDPData(_packet);
        }
    }
    /* // UDP test 
    public static void UDPTestReceived()
    {
        using (Packet _packet = new Packet((int) ClientPackets.udpTestReceive))
        {
            _packet.Write($"Received a UDP packet.");
            SendUDPData(_packet);
        }
    }
    */
    #endregion
}
