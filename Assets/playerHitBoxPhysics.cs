using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class playerHitBoxPhysics : NetworkBehaviour
{
    [SerializeField] private Transform CameraHead;
    [SerializeField] private Vector3 offsev;

    public int CurrentHealth;
    public int MaxHealth = 100;
    public enum StatusEffects
    {
        None,
        Freeze,
        Stun, //use very rarely
        NearSight,

    }

    private void Start()
    {
        CurrentHealth = MaxHealth;
    }

    void LateUpdate()
    {
        if (IsOwner)
            return;


        transform.position = CameraHead.position + offsev;
    }


    public void AffectPlayer(int dmg)
    {
        AffectPlayer(dmg, 0, Vector3.zero, StatusEffects.None);
    }
    public void AffectPlayer(float force, Vector3 direct)
    {
        AffectPlayer(0, force, direct, StatusEffects.None);
    }
    public void AffectPlayer(int dmg, StatusEffects effect)
    {
        AffectPlayer(dmg, 0, Vector3.zero, effect);
    }
    public void AffectPlayer(int dmg, float force, Vector3 direct)
    {
        AffectPlayer(dmg, force, direct, StatusEffects.None);
    }
    public void AffectPlayer(float force, Vector3 direct, StatusEffects effect)
    {
        AffectPlayer(0, force, direct, effect);
    }
    public void AffectPlayer(int dmg, float force, Vector3 direct, StatusEffects effect)
    {
        if(dmg != 0) //dmg part
        {

        }
        if(force != 0) //force part
        {

        }
        if(effect != StatusEffects.None) //effect part
        {

        }
    }

}
