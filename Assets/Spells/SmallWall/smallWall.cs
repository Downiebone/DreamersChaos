using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[CreateAssetMenu]
public class smallWall : spell
{
    [SerializeField] private GameObject wall;

    public override void Activate(GameObject parent, Transform firePoint)
    {
        SpawnObject(wall.name, firePoint.position, parent.transform.rotation);
        //GameObject GO = Instantiate(wall, firePoint.position, parent.transform.rotation);
        //GO.GetComponent<NetworkObject>().Spawn(true);
    }
}
