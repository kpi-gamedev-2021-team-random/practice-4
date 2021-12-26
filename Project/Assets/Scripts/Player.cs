using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    [SyncVar(hook = nameof(SyncHealth))]
    private int _SyncHealth;

    public GameObject BulletPrefab;

    private NetworkManagerShooter _networkManager;

    private Text _hpText;

    private SpriteRenderer _spriteRenderer;

    private int _health = 100;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _hpText = GameObject.Find("HP_T").GetComponent<Text>();

        _hpText.text = "HP: " + _health;
    }

    public void Init(NetworkManagerShooter networkManagerShooter)
    {
        _networkManager = networkManagerShooter;
    }

    void Update()
    {
        if (hasAuthority)
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            float speed = 10f * Time.deltaTime;
            transform.Translate(new Vector2(0, v * speed));
            transform.localEulerAngles += new Vector3(0, 0, h * -Time.deltaTime * 250);


            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (isServer)
                    SpawnBullet(netId, transform.up);
                else
                    CmdSpawnBullet(netId, transform.up);
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                Debug.Log("SHUTDOWN");

                ShutDown();
            }
        }
    }

    private void ShutDown()
    {
        if (isServer)
        {
            ShutdownServer();
        }
        else
        {
            CmdShutdownClient();
        }
    }

    [Server]
    private void ShutdownServer()
    {
        if (_networkManager.IsAllAreDead)
            NetworkServer.Shutdown();
    }

    [Command]
    private void CmdShutdownClient()
    {
        if(_networkManager.IsAllAreDead)
            NetworkServer.Shutdown();
    }


    [Server]
    public void SpawnBullet(uint owner, Vector3 direction)
    {
        GameObject bulletGo = Instantiate(BulletPrefab, transform.position, Quaternion.identity); 
        NetworkServer.Spawn(bulletGo);
        bulletGo.GetComponent<Bullet>().Init(owner, direction);
    }

    [Command]
    public void CmdSpawnBullet(uint owner, Vector3 direction)
    {
        SpawnBullet(owner, direction);
    }

    [ClientRpc]
    public void DamageFromServer(int value)
    {
        ChangeHealthVisualization(_health - value);

        ChangeHealthValue(_health - value);
    }

    private void SyncHealth(int oldValue, int newValue)
    {
        _health = newValue;
    }

    private void ChangeHealthVisualization(int value)
    {
        if (hasAuthority)
        {
            _hpText.text = "HP: " + value;

            if (value <= 25)
            {
                _spriteRenderer.color = Color.red;
            }
        }
    }

    [Server]
    private void ChangeHealthValue(int newValue)
    {
        _SyncHealth = newValue;

        if (_SyncHealth <= 0)
        {
            _networkManager.Dead();

            NetworkServer.Destroy(gameObject);
        }
    }
}
