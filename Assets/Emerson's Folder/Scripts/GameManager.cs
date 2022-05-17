using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    // stores all player info on the client side
    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();
    // Gameobjects for the local and player prefab
    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;
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
    // method to spawn the player
    public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation)
    {
        GameObject _player;
        // if its a local player
        if (_id == Client.instance.myId)
        {
            _player = Instantiate(localPlayerPrefab, _position, _rotation);
        }
        else
        {
            _player = Instantiate(playerPrefab, _position, _rotation);
        }
        // assigns the information of the player to the created prefab
        _player.GetComponent<PlayerManager>().id = _id;
        _player.GetComponent<PlayerManager>().username = _username;
        players.Add(_id, _player.GetComponent<PlayerManager>());
    }
}
