using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class basic_projectile : NetworkBehaviour
{
    [SerializeField] protected float speed = 10;
    [SerializeField][Tooltip("0 = never despawn on its own")] protected float timeBeforeDeath = 5;
    [SerializeField] protected bool spawnWithoutHittingAnything = false;
    [SerializeField] protected GameObject instUponDeath = null;
    [SerializeField] protected bool canHitSelf = false;
    [SerializeField] protected bool willDestroyObjects = false;
    [Range(1, 1000)]
    [SerializeField] protected int destroyObjectsRange = 2;

    [SerializeField] protected LayerMask hitGroundLayer;

    protected void Start()
    {
        if (spawnWithoutHittingAnything && timeBeforeDeath != 0)
        {
            Invoke("projectileDeathSpawn", timeBeforeDeath);
        }
    }

    protected void Update()
    {
        transform.position += transform.forward * Time.deltaTime * speed;
    }

    protected void projectileDeathSpawn()
    {
        

        if (!NetworkManager.Singleton.IsServer) return;  // do server things

        if(willDestroyObjects == true)
        {
            //WorldGenerator.Instance?.ModifyChunkFastGameExplosion(transform.position, destroyObjectsRange);
            Collider[] cols = Physics.OverlapSphere(transform.position, destroyObjectsRange * 0.75f, hitGroundLayer, QueryTriggerInteraction.Ignore);
            foreach(Collider c in cols)
            {
                NetworkObject ob = c.GetComponentInParent<NetworkObject>();
                Destroy(c.gameObject);
                c.GetComponentInParent<NetworkObject>().Despawn();
            }
            
            ChunkExplosionClientRpc(transform.position, destroyObjectsRange);
        }

        Destroy(this.gameObject);
    }


    [ClientRpc]
    private void ChunkExplosionClientRpc(Vector3 pos, int range)
    {
        if (instUponDeath != null)
        {
            Instantiate(instUponDeath, transform.position, transform.rotation);
        }
        WorldGenerator.Instance?.ModifyChunkFastGameExplosion(pos, range);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!NetworkManager.Singleton.IsServer) return;  // do server things

        if(other.gameObject.layer == 9 && other.gameObject.GetComponentInParent<NetworkObject>() != null)
        {
            if (canHitSelf || (GetComponent<NetworkObject>().OwnerClientId != other.gameObject.GetComponentInParent<NetworkObject>().OwnerClientId))
            {
                projectileDeathSpawn();
            }
        }
        else
        {
            projectileDeathSpawn();
        }

    }
}
