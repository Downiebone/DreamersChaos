using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class dashAbility : spell
{
    [SerializeField] int flightForce;

    public override void Activate(GameObject parent, Transform firePoint)
    {
        Rigidbody rb = GetRigidbody(parent);
        rb.velocity = new Vector3(rb.velocity.x / 2, rb.velocity.y / 3, rb.velocity.z / 2);
        rb.AddForce(Vector3.up * flightForce, ForceMode.Impulse);
    }

    public override void CancelSpell(GameObject parent, Transform firePoint)
    {
        Rigidbody rb = GetRigidbody(parent);
        if (rb.velocity.y <= 0) return;
        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y / 2, rb.velocity.z);
    }


}
