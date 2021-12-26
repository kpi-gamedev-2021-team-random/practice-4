using Mirror;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    private uint _owner;
    private bool _inited;

    private Vector3 _direction;
    private Vector3 _spawnPoint;

    [SerializeField] private int _damage = 25;

    [Server]
    public void Init(uint owner, Vector3 direction)
    {
        _owner = owner;
        _direction = direction; 

        _spawnPoint = transform.position;

        _inited = true;
    }

    private void Update()
    {
        if (_inited && isServer)
        {
            transform.position += _direction * Time.deltaTime * 40;

            Collide();

            if (Vector3.Distance(transform.position, _spawnPoint) > 60)
            {
                NetworkServer.Destroy(gameObject); 
            }
        }
    }

    private void Collide()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.5f);

        for (int i = 0; i < colliders.Length; i++)
        {
            Player player = colliders[i].GetComponent<Player>();

            if (player != null)
            {
                if (player.netId != _owner)
                {
                    player.DamageFromServer(_damage);
                    NetworkServer.Destroy(gameObject);
                }
            }
        }
    }
}