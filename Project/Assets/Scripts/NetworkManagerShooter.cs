using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerShooter : NetworkManager
{
    [SerializeField] private Transform[] _spawnPoints;

    [SerializeField] private GameObject _restartText;

    [Header("Lobby canvas")]
    [SerializeField] private GameObject _canvas;

    [SerializeField] private Text _playersJoined;

    private List<Player> _players = new List<Player>();

    private List<NetworkConnection> _networkConnections = new List<NetworkConnection>();

    private int _playerSpawned;

    public int DeadPlayers { get; private set; }

    public int GetPlayers { get => _playerSpawned; }

    public bool IsAllAreDead { get => _playerSpawned - 1 <= DeadPlayers; }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        if (_networkConnections.Count < 4)
        {
            _networkConnections.Add(conn);

            _playersJoined.text = "Players: " + _networkConnections.Count;
        }
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        _networkConnections.Remove(conn);

        _playersJoined.text = "Players: " + _networkConnections.Count;

        base.OnClientDisconnect(conn);
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        _canvas.SetActive(true);

        base.OnServerConnect(conn);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        _playerSpawned = 0;

        _restartText.SetActive(false);

        base.OnServerDisconnect(conn);
    }

    public void OnCreateCharacter(NetworkConnection conn)
    {
        Transform spawn = _spawnPoints[_playerSpawned];

        GameObject player = Instantiate(playerPrefab, spawn.position, spawn.rotation);

        player.GetComponent<Player>().Init(this);

        _players.Add(player.GetComponent<Player>());

        NetworkServer.AddPlayerForConnection(conn, player);

        if (_spawnPoints.Length >= _playerSpawned + 1)
        {
            _playerSpawned++;
        }
    }


    public void StartGame()
    {
        if (_networkConnections.Count > 1 && _networkConnections.Count <= 4)
        {
            for (int i = 0; i < _networkConnections.Count; i++)
            {
                OnCreateCharacter(_networkConnections[i]);
            }

            _networkConnections = new List<NetworkConnection>();

            _canvas.SetActive(false);
        }
    }

    public void Dead()
    {
        Debug.Log("Dead");

        DeadPlayers++;

        if (IsAllAreDead)
        {
            _restartText.SetActive(true);
        }
    }
}