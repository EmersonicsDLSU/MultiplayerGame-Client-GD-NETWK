using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();
    public static Dictionary<int, ItemSpawner> itemSpawners = new Dictionary<int, ItemSpawner>();
    public static Dictionary<int, ProjectileManager> projectiles = new Dictionary<int, ProjectileManager>();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;
    public GameObject itemSpawnerPrefab;
    public GameObject projectilePrefab;
    
    public GameObject mainCamera;

    // timer
    [HideInInspector]public float _timeTick;
    [HideInInspector]public float _maxTime;
    [HideInInspector]public bool _isGameFinished;

    [SerializeField] private List<Material> colorMaterials;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    /// <summary>Spawns a player.</summary>
    /// <param name="_id">The player's ID.</param>
    /// <param name="_name">The player's name.</param>
    /// <param name="_position">The player's starting position.</param>
    /// <param name="_rotation">The player's starting rotation.</param>
    public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation, int _playerColor)
    {
        GameObject _player;
        if (_id == Client.instance.myId)
        {
            _player = Instantiate(localPlayerPrefab, _position, _rotation);
        }
        else
        {
            _player = Instantiate(playerPrefab, _position, _rotation);
        }
        
        // playerInfoPanel
        var tempObj = 
            Instantiate(UIManager.instance.playerInfoPanelPrefab, Vector3.zero, Quaternion.identity);
        UIManager.instance.playerInfoList.Add(tempObj);
        tempObj.transform.parent = UIManager.instance.playersInfoPanel.transform;
        _player.GetComponent<PlayerManager>().playerNameScore = tempObj.GetComponentInChildren<Text>();
        // change player playerInfoPanel color
        tempObj.
            GetComponent<Image>().color = SetPanelColor((PlayerColors) _playerColor);
        // change player meshRenderer color
        _player.GetComponentInChildren<MeshRenderer>().material =
            SetPlayerColor((PlayerColors) _playerColor);
        // assign playerColor
        _player.GetComponent<PlayerManager>().playerColor = (PlayerColors) _playerColor;
        
        _player.GetComponent<PlayerManager>().Initialize(_id, _username);
        players.Add(_id, _player.GetComponent<PlayerManager>());
    }
    
    public static Color SetPanelColor(PlayerColors _playerColor)
    {
        switch (_playerColor)
        {
            case PlayerColors.RED:
                return Color.red;
                break;
            case PlayerColors.BLUE:
                return Color.blue;
                break;
            case PlayerColors.GREEN:
                return Color.green;
                break;
            case PlayerColors.WHITE:
                return Color.white;
                break;
            case PlayerColors.BLACK:
                return Color.black;
                break;
            case PlayerColors.YELLOW:
                return Color.yellow;
                break;
            default:
                return Color.black;
        }
    }
    private Material SetPlayerColor(PlayerColors _playerColor)
    {
        switch (_playerColor)
        {
            case PlayerColors.RED:
                return colorMaterials[0];
                break;
            case PlayerColors.BLUE:
                return colorMaterials[1];
                break;
            case PlayerColors.GREEN:
                return colorMaterials[2];
                break;
            case PlayerColors.WHITE:
                return colorMaterials[3];
                break;
            case PlayerColors.BLACK:
                return colorMaterials[4];
                break;
            case PlayerColors.YELLOW:
                return colorMaterials[5];
                break;
            default:
                return colorMaterials[colorMaterials.Count - 1];
        }
    }

    public void CreateItemSpawner(int _spawnerId, Vector3 _position, bool _hasItem)
    {
        GameObject _spawner = Instantiate(itemSpawnerPrefab, _position, itemSpawnerPrefab.transform.rotation);
        _spawner.GetComponent<ItemSpawner>().Initialize(_spawnerId, _hasItem);
        itemSpawners.Add(_spawnerId, _spawner.GetComponent<ItemSpawner>());
    }

    public void SpawnProjectile(int _id, Vector3 _position)
    {
        GameObject _projectile = Instantiate(projectilePrefab, _position, Quaternion.identity);
        _projectile.GetComponent<ProjectileManager>().Initialize(_id);
        projectiles.Add(_id, _projectile.GetComponent<ProjectileManager>());
    }

    public void UpdateKillCount(int _id, int _killCount)
    {
        // assign the new killCount
        players[_id].killCount = _killCount;
    }

    public void PlayerDisconnected(int _id)
    {
        // remove panelObj
        var panelGO = UIManager.instance.playerInfoList.Find((x) =>
            x.GetComponentInChildren<Text>().text == players[_id].username);
        UIManager.instance.playerInfoList.Remove(panelGO);
        Destroy(panelGO);
        // remove capsule object
        Destroy(players[_id].gameObject);
        players.Remove(_id);
    }
    
}