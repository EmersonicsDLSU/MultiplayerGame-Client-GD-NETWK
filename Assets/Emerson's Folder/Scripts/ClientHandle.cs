using System;
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
        int _playerColor = _packet.ReadInt();
        // call this method to create and spawn the new player
        GameManager.instance.SpawnPlayer(_id, _username, _position, _rotation, _playerColor);
    }
    public static void PlayerPosition(Packet _packet)
    {
        // reads the packet which contains the id and updated position
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        bool _isWalking = _packet.ReadBool();
        bool _isJumping = _packet.ReadBool();
        bool _isGround = _packet.ReadBool();
        // updates the position in the local game
        GameManager.players[_id].transform.position = _position;
        // update ground position
        GameManager.players[_id].isGround = _isGround;
        // play sound for movement
        if (_isJumping && _isGround)
        {
            AudioManager.Instance.Stop(SoundCode.WALK_SOUND);
            AudioManager.Instance.Play(SoundCode.JUMP_SOUND);
        }
        else if (_isWalking && _isGround && 
                 !AudioManager.Instance.GetSound(SoundCode.WALK_SOUND).source.isPlaying)
        {
            AudioManager.Instance.Play(SoundCode.WALK_SOUND);
        }
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

        GameManager.instance.PlayerDisconnected(_id);
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
    public static void CreateItemSpawner(Packet _packet)
    {
        int _spawnerId = _packet.ReadInt();
        Vector3 _spawnerPosition = _packet.ReadVector3();
        bool _hasItem = _packet.ReadBool();

        GameManager.instance.CreateItemSpawner(_spawnerId, _spawnerPosition, _hasItem);
    }

    public static void ItemSpawned(Packet _packet)
    {
        int _spawnerId = _packet.ReadInt();

        GameManager.itemSpawners[_spawnerId].ItemSpawned();
    }

    public static void ItemPickedUp(Packet _packet)
    {
        int _spawnerId = _packet.ReadInt();
        int _byPlayer = _packet.ReadInt();

        GameManager.itemSpawners[_spawnerId].ItemPickedUp();
        GameManager.players[_byPlayer].itemCount++;
    }
    public static void SpawnProjectile(Packet _packet)
    {
        int _projectileId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        int _thrownByPlayer = _packet.ReadInt();

        // call throw sound
        AudioManager.Instance.Play(SoundCode.THROW_SOUND);
        GameManager.instance.SpawnProjectile(_projectileId, _position);
        GameManager.players[_thrownByPlayer].itemCount--;
    }

    public static void ProjectilePosition(Packet _packet)
    {
        int _projectileId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        GameManager.projectiles[_projectileId].transform.position = _position;
    }

    public static void ProjectileExploded(Packet _packet)
    {
        int _projectileId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        GameManager.projectiles[_projectileId].Explode(_position);
    }
    public static void PlayerKillPoint(Packet _packet)
    {
        int _id = _packet.ReadInt();
        int _killCount = _packet.ReadInt();

        GameManager.instance.UpdateKillCount(_id, _killCount);
    }
    public static void UpdateGameManager(Packet _packet)
    {
        float _timeTick = _packet.ReadFloat();
        float _maxTime = _packet.ReadFloat();
        bool _isGameFinished = _packet.ReadBool();

        GameManager.instance._timeTick = _timeTick;
        GameManager.instance._maxTime = _maxTime;
        GameManager.instance._isGameFinished = _isGameFinished;
        UIManager.instance.timerText.text = 
            $"Time Left: {Math.Round(_timeTick, 1)}";

        if (_isGameFinished)
        {
            Debug.LogError($"GameFinish!");
            UIManager.instance.ShowWinner();
        }
    }
    public static void ResetGame(Packet _packet)
    {
        bool _isGameFinished = _packet.ReadBool();

        GameManager.instance._isGameFinished = _isGameFinished;
        UIManager.instance.winnerPanel.SetActive(false);
        UIManager.instance.waitPanel.SetActive(false);
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
