using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_spellHander : MonoBehaviour
{
    public static UI_spellHander Instance;

    [SerializeField] private GameObject UI_Conten_Holder;
    private List<GameObject> spellsInUI = new List<GameObject>();
    [SerializeField] private GameObject spellSelectScreen;
    [SerializeField] private GameObject spellMenuScreen;

    [Header("UI_spell_inst")]
    [SerializeField] private GameObject UI_spell_Prefab;

    [HideInInspector] public spellList spell_Info;

    [Header("HotBar")]
    [SerializeField] private hotBarScript hotBar;

    [Header("setting icon")]
    [SerializeField] private Image settingIcon;
    private spell spellToInsert;
    private Sprite spellIconToInsert;

    [SerializeField] private pauseManager pauseScript;

    public void addRandomSpell()
    {
        List<string> potential_spells = new List<string>();

        foreach (var spelInf in spell_Info.allSpellList)
        {
            if (spelInf.banned) { continue; }
            if (spelInf.owned) { continue; }

            potential_spells.Add(spelInf.spell_script.name);
        }

        addNewSpell(potential_spells[Random.Range(0, potential_spells.Count)]);
    }
    public void addNewSpell(string spellsName)
    {
        
        for (var i = 0; i < spell_Info.allSpellList.Length; i++)
        {
            if (spell_Info.allSpellList[i].banned) { continue; }
            if (spell_Info.allSpellList[i].owned) { continue; }

            if (spell_Info.allSpellList[i].spell_script.name == spellsName)
            {
                GameObject GO = Instantiate(UI_spell_Prefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
                GO.transform.parent = UI_Conten_Holder.transform;
                GO.transform.GetChild(0).GetComponent<Image>().sprite = spell_Info.allSpellList[i].spell_script.spellIcon; // set sprite of item
                GO.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = (spell_Info.allSpellList[i].spell_script.cooldownTime).ToString(); //find spells cooldown, and display that

                GO.GetComponent<Button>().onClick.AddListener(delegate { insertNewSpell(spell_Info.allSpellList[i].spell_script); }); //set onclick of this item

                spell_Info.allSpellList[i].owned = true;

                break;
            }
        }
        //foreach (spellList.NamedImage spelInf in spell_Info.allSpellList)
        //{
        //    if (spelInf.banned) { continue; }
        //    if (spelInf.owned) { continue; }

        //    if (spelInf.spell_script.name == spellsName) {
        //        GameObject GO = Instantiate(UI_spell_Prefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
        //        GO.transform.parent = UI_Conten_Holder.transform;
        //        GO.transform.GetChild(0).GetComponent<Image>().sprite = spelInf.spell_script.spellIcon; // set sprite of item
        //        GO.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = (spelInf.spell_script.cooldownTime).ToString(); //find spells cooldown, and display that

        //        GO.GetComponent<Button>().onClick.AddListener(delegate { insertNewSpell(spelInf.spell_script); }); //set onclick of this item

        //        spelInf.owned = true;

        //        break;
        //    }
        //}
    }
    private void Awake()
    {
        Instance = this;

        spell_Info = GameObject.FindGameObjectWithTag("spellList").GetComponent<spellList>();

        foreach (var spelInf in spell_Info.allSpellList)
        {
            if (spelInf.banned) { continue; }

            if (spelInf.owned == false) { continue; }

            GameObject GO = Instantiate(UI_spell_Prefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
            GO.transform.parent = UI_Conten_Holder.transform;
            GO.transform.GetChild(0).GetComponent<Image>().sprite = spelInf.spell_script.spellIcon; // set sprite of item
            GO.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = (spelInf.spell_script.cooldownTime).ToString(); //find spells cooldown, and display that
            
            GO.GetComponent<Button>().onClick.AddListener(delegate { insertNewSpell(spelInf.spell_script); }); //set onclick of this item
        }
    }

    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(1)) && !spellMenuScreen.activeInHierarchy && pauseScript.isPaused == false) //open spell menu, if spell menu is not open, and youre not already in pause menu
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            spellMenuScreen.gameObject.SetActive(true);
            pauseScript.isPaused = true;
        }
        else if(Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(1)) //if youre looking in menu
        {
            spellMenuScreen.gameObject.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            pauseScript.isPaused = false;
        }
    }

    public void insertNewSpell(spell spellToIns)
    {
        if (hotBar.canUseHotbar == false)
            return;

        spellSelectScreen.gameObject.SetActive(false);
        hotBar.setSpell(spellToIns);

        spellMenuScreen.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        pauseScript.isPaused = false;
    }
}
