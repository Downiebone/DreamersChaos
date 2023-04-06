using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spell : ScriptableObject
{
    public new string name;
    public Sprite spellIcon;
    public string description;
    public string[] searchTags;
    public string[] deathTexts;
    public Color UI_color = Color.blue;

    public bool inQueue;
    public float castTime = 0;

    public float cooldownTime;
    public float currentCooldown = 0;

    public bool hasActiveTime;
    public bool canCancelActiveTime;
    public float activeTime;
    public float currentActiveTime = 0;

    protected void SpawnObject(string objectName, Vector3 pos, Quaternion rot)
    {
        GameObject.FindGameObjectWithTag("MyPlayer").GetComponent<setUpOwnerPlayer>().callAnRCP_spawnObject(objectName, pos, rot);
    }
    public void resetSpellValues()
    {
        currentActiveTime = activeTime; //does things when current < active time (this script makes so it starts by doing nothing)
        currentCooldown = 0;
        inQueue = false;
    }
    #region basic_spell_features
    public virtual void preCastTime_Activate(GameObject parent, Transform firePoint) { }
    public virtual void Activate(GameObject parent, Transform firePoint) { }
    public virtual void CancelSpell(GameObject parent, Transform firePoint) { }
    //public virtual void longActive(GameObject parent, Transform firePoint) { }
    public virtual void Indicator(GameObject parent, Transform firePoint) { }
    #endregion

    #region quality_of_life_things
    protected Rigidbody GetRigidbody(GameObject parent)
    {
        return parent.transform.parent.GetComponent<Rigidbody>();
    }
    #endregion
}
