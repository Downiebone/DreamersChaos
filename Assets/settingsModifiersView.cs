using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class settingsModifiersView : MonoBehaviour
{
    [SerializeField] private TMP_Text StartWithSPells_num;
    [SerializeField] private TMP_Text cooldownModUI_percent;
    [SerializeField] private TMP_Text knockbackModUI_percent;
    [SerializeField] private TMP_Text damageModUI_percent;

    public GameObject spellList;

    private int startWithSpells = 0;
    private int maxSpells = 10; //temp
    private float CooldownMod = 1f;
    private float KockbackMod = 1f;
    private float DamageMod = 1f;

    private void Start()
    {
        updateUIs();
    }

    public void UpdateSpellAmmount(bool increase)
    {
        if (increase)
        {
            startWithSpells += 1;
            if (startWithSpells > maxSpells)
            {
                startWithSpells = 0;
            }
        }
        else
        {
            startWithSpells -= 1;
            if (startWithSpells < 0)
            {
                startWithSpells = maxSpells;
            }
        }
        updateUIs();
    }
    public void UpdateCooldownMod(bool increase)
    {
        if (increase)
        {
            CooldownMod += 0.1f;
        }
        else
        {
            CooldownMod -= 0.1f;
            if (CooldownMod < 0)
            {
                CooldownMod = 0;
            }
        }
        updateUIs();
    }
    public void UpdateKnockbackMod(bool increase)
    {
        if (increase)
        {
            KockbackMod += 0.1f;
        }
        else
        {
            KockbackMod -= 0.1f;
            if (KockbackMod < 0)
            {
                KockbackMod = 0;
            }
        }
        updateUIs();
    }
    public void UpdateDamageMod(bool increase)
    {
        if (increase)
        {
            DamageMod += 0.1f;
        }
        else
        {
            DamageMod -= 0.1f;
            if (DamageMod < 0)
            {
                DamageMod = 0;
            }
        }
        updateUIs();
    }
    public void resetValues()
    {
        CooldownMod = 1f;
        KockbackMod = 1f;
        DamageMod = 1f;
        updateUIs();
    }
    private void updateUIs()
    {
        StartWithSPells_num.text = startWithSpells == maxSpells ? "ALL" : startWithSpells.ToString();
        cooldownModUI_percent.text = CooldownMod == 0 ? "None" : Mathf.RoundToInt(CooldownMod * 100) + "%";
        knockbackModUI_percent.text = KockbackMod == 0 ? "None" : Mathf.RoundToInt(KockbackMod * 100) + "%";
        damageModUI_percent.text = DamageMod == 0 ? "None" : Mathf.RoundToInt(DamageMod * 100) + "%";
    }




    public void openSpellList()
    {
        spellList.gameObject.SetActive(true);
    }


    public void applyAndBack()
    {
        PlayerPrefs.SetInt("START_SPELLS", startWithSpells);
        PlayerPrefs.SetFloat("KNOCKBACK_MOD", KockbackMod);
        PlayerPrefs.SetFloat("DAMAGE_MOD", DamageMod);
        PlayerPrefs.SetFloat("COOLDOWN_MOD", CooldownMod);

        this.gameObject.SetActive(false);
    }

}
