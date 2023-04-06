using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "spawnObj",menuName = "spawn Obj")]
public class simpleSpawn : spell
{
    [Header("Object specific")]
    [SerializeField] private GameObject object_toSpawn;
    [SerializeField] private bool spawnInLookDirection = true;

    public override void Activate(GameObject parent, Transform firePoint)
    {
        if(spawnInLookDirection)
            SpawnObject(object_toSpawn.name, firePoint.position, firePoint.rotation);
        else
            SpawnObject(object_toSpawn.name, firePoint.position, parent.transform.rotation);
    }
}
