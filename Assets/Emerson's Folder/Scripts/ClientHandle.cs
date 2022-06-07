using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{

    public static void Welcome(Packet _packet)
    {
        // reads the string message from the _packet
        string _msg = _packet.ReadString();
        // reads the integer ID from the _packet
        int _myId = _packet.ReadInt();
        // debug to display the message that the client received from the server
        Debug.Log($"Message from server: {_msg}");
        // sets the ID sent to the client's ID field
        Client.instance.myId = _myId;
        // sends a packet(message) back to the server to tell that we've received the message
        ClientSend.WelcomeReceived();
        // pass in the local port to the udp, that our TCP connection is using
        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnPlayer(Packet _packet)
    {
        // read all the info of the player from the packet
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();
        // call this method to create and spawn the new player
        GameManager.instance.SpawnPlayer(_id, _username, _position, _rotation);
    }
    public static void PlayerPosition(Packet _packet)
    {
        // reads the packet which contains the id and updated position
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        // updates the position in the local game
        GameManager.players[_id].transform.position = _position;
    }

    public static void PlayerRotation(Packet _packet)
    {
        // reads the packet which contains the id and updated rotation
        int _id = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuaternion();
        // updates the rotation in the local game
        GameManager.players[_id].transform.rotation = _rotation;
    }

    public static void PlayerDisconnected(Packet _packet)
    {
        int _id = _packet.ReadInt();

        Destroy(GameManager.players[_id].gameObject);
        GameManager.players.Remove(_id);
    }
    // reads the player's health coming from the server
    public static void PlayerHealth(Packet _packet)
    {
        int _id = _packet.ReadInt();
        float _health = _packet.ReadFloat();

        GameManager.players[_id].SetHealth(_health);
    }
    
    // reads the player's respawned condition coming from the server
    public static void PlayerRespawned(Packet _packet)
    {
        int _id = _packet.ReadInt();

        GameManager.players[_id].Respawn();
    }

    /* // UDP Test
    public static void UDPTest(Packet _packet)
    {
        // read the string
        string _msg = _packet.ReadString();

        Debug.Log($"Received packet via UDP. Contains message: {_msg}");
        ClientSend.UDPTestReceived();
    }
    */
}
