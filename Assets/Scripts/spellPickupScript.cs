using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spellPickupScript : MonoBehaviour
{
    private spellList spell_list;
    private UI_spellHander UI_and_SpellHander;

    [SerializeField] private float rotateSpeed;
    [SerializeField] private SpriteRenderer sprit1,sprit2;
    public string spellNamePick;
    [SerializeField] private float downWardsVelocity = 1;

    private int itemsIndex;
    private int startVal;

    private void Awake()
    {
        spell_list = GameObject.FindGameObjectWithTag("spellList").GetComponent<spellList>();
        UI_and_SpellHander = GameObject.FindGameObjectWithTag("UI_SpellHandler").GetComponent<UI_spellHander>();

        itemsIndex = Random.Range(0, spell_list.allSpellList.Length);
        startVal = itemsIndex;

        becomeSpell();

        GetComponent<Rigidbody>().velocity = new Vector3(0, -downWardsVelocity, 0);
    }

    private void becomeSpell()
    {
        if (spell_list.allSpellList[itemsIndex].banned) //if spell is banned --> become spell before that in list --> repeat
        {
            if(itemsIndex > 0) { itemsIndex--; }
            else { itemsIndex = (spell_list.allSpellList.Length - 1); }

            if(itemsIndex == startVal) { Destroy(this.gameObject); return; } //if no spell was unbanned, --> destroy

            becomeSpell();
            return;
        }

        spellNamePick = spell_list.allSpellList[itemsIndex].spell_script.name;
        Sprite newSprit = spell_list.allSpellList[itemsIndex].spell_script.spellIcon;
        sprit1.sprite = newSprit;
        sprit2.sprite = newSprit;
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (true)//if (MY_PLAYER)  do this only if it is my player that stepped on this
        {
            int index = -1;
            foreach (var item in spell_list.allSpellList)
            {
                index++;
                if(item.spell_script.name != spellNamePick) { continue; }
                if(item.owned == true) { continue; }

                //only if you dont own the item
                spell_list.allSpellList[index].owned = true;
                UI_and_SpellHander.addNewSpell(spell_list.allSpellList[index].spell_script.name);

                Destroy(this.gameObject); // DESTROY THIS FOR EVERY PLAYER
            }
        }
    }
}
