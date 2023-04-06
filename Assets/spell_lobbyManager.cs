using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class spell_lobbyManager : MonoBehaviour
{
    [SerializeField] private GameObject UI_Conten_Holder;
    [SerializeField] private GameObject UI_spell_Prefab;
    private spellList spell_Info;

    List<lobbySpell_info> spellinfos = new List<lobbySpell_info>();
    private void Awake()
    {
        spell_Info = GameObject.FindGameObjectWithTag("spellList").GetComponent<spellList>();

        foreach (var spelInf in spell_Info.allSpellList)
        {
            GameObject GO = Instantiate(UI_spell_Prefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
            GO.transform.parent = UI_Conten_Holder.transform;

            lobbySpell_info lobbyInfObj = GO.GetComponent<lobbySpell_info>();
            spellinfos.Add(lobbyInfObj);

            lobbyInfObj.setEverything(spelInf.spell_script, spelInf.banned);

            GO.GetComponent<Button>().onClick.AddListener(delegate { banOrunbanSpell(lobbyInfObj); }); //set onclick of this item
        }
    }

    public void banOrunbanSpell(lobbySpell_info infoObj)
    {
        infoObj.updateUI();
    }
    public void closeSpellList()
    {
        for(var i = 0; i < spellinfos.Count; i++)
        {
            spell_Info.allSpellList[i].banned = spellinfos[i].is_banned;
        }


        this.gameObject.gameObject.SetActive(false);
    }
}
