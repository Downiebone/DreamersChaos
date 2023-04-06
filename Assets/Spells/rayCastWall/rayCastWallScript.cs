using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class rayCastWallScript : spell
{

    [SerializeField] private GameObject objToSpawn;
    public override void Activate(GameObject parent, Transform firePoint)
    {
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(firePoint.position, firePoint.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
        {
            Quaternion rot = Quaternion.LookRotation(hit.normal) * Quaternion.FromToRotation(Vector3.forward, -Vector3.up);
            SpawnObject(objToSpawn.name, hit.point, rot);
        }
    }
}
