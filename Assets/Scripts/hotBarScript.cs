using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class hotBarScript : MonoBehaviour
{
    public static hotBarScript Instance;

    [SerializeField] private emtySpell emptySpell_filler;
    [SerializeField] private Sprite emptySpell_Sprite;
    [SerializeField] private GameObject parent_Player;
    [SerializeField] private Transform firePoint;
    [SerializeField] private spellCastSlider castSpellUI;

    public int hotBarSlots = 5;
    public List<spell> chosenSpells = new List<spell>();
    public List<spell> queuedSpells = new List<spell>();
    [SerializeField] private GameObject[] hotBars;
 
    private int hotBarSelected = 0;

    [HideInInspector] public pauseManager pauseScript;

    private KeyCode[] keyCodes = {
         KeyCode.Alpha1,
         KeyCode.Alpha2,
         KeyCode.Alpha3,
         KeyCode.Alpha4,
         KeyCode.Alpha5,
         KeyCode.Alpha6,
         KeyCode.Alpha7,
         KeyCode.Alpha8,
         KeyCode.Alpha9,
     };


    private IEnumerator currentlyCasting_coroutine;
    private bool currently_casting;


    [HideInInspector] public bool canUseHotbar = false;


    private void Awake()
    {
        Instance = this;
    }

    public void Init(GameObject oriantation, Transform FirePoint)
    {
        parent_Player = oriantation;
        firePoint = FirePoint;
    }

    public void initializeUseOfHotbar()
    {
        canUseHotbar = true;
    }

    void Start()
    {
        pauseScript = GameObject.FindGameObjectWithTag("pauseManager").GetComponent<pauseManager>();

        for (var i = 0; i < hotBars.Length; i++)
        {
            if(i > (hotBarSlots - 1))
            {
                hotBars[i].gameObject.SetActive(false);
            }
            else
            {
                hotBars[i].gameObject.SetActive(true);
            }
        }

        foreach (spell spelle in chosenSpells)
        {
            
            spelle.resetSpellValues();
            
        }

        updateUI_selected();
    }

    private void addSpellToQueue(spell spell)
    {
        if (currently_casting == false)
        {
            currently_casting = true;
            spell.inQueue = true;
            currentlyCasting_coroutine = castSpell(spell);
            StartCoroutine(currentlyCasting_coroutine);
        }
        else
        {
            if (spell.hasActiveTime) return; //cant queue spells with active-time

            spell.inQueue = true;
            queuedSpells.Add(spell);
        }   
    }

    void Update()
    {
        if (!canUseHotbar)
            return;

        #region spellHandler
        spell selectedSpell = chosenSpells[hotBarSelected];

        if (Input.GetKeyDown(KeyCode.Mouse0) && pauseScript.isPaused == false) //cant shoot if paused
        {
            if(selectedSpell.currentCooldown >= (selectedSpell.cooldownTime * PlayerPrefs.GetFloat("COOLDOWN_MOD")) && selectedSpell.inQueue == false)
            {
                addSpellToQueue(selectedSpell);
            }
            else if(selectedSpell.hasActiveTime && selectedSpell.canCancelActiveTime && selectedSpell.currentActiveTime < selectedSpell.activeTime)
            {
                selectedSpell.CancelSpell(parent_Player, firePoint);
                selectedSpell.currentActiveTime = selectedSpell.activeTime;
            }
        }

        selectedSpell.Indicator(parent_Player,  firePoint);

        for(var i = 0; i < chosenSpells.Count; i++)
        {
            if (!chosenSpells[i].hasActiveTime || chosenSpells[i].currentActiveTime >= chosenSpells[i].activeTime)
            {

                if (chosenSpells[i].currentCooldown < (chosenSpells[i].cooldownTime * PlayerPrefs.GetFloat("COOLDOWN_MOD")))
                {
                    chosenSpells[i].currentCooldown += Time.deltaTime;
                    float UI_overlayShade_val = 1f - (chosenSpells[i].currentCooldown / (chosenSpells[i].cooldownTime * PlayerPrefs.GetFloat("COOLDOWN_MOD")));

                    hotBars[i].transform.GetChild(1).GetComponent<Image>().fillAmount = UI_overlayShade_val;
                    //Debug.Log("cooldown: " + chosenSpells[i].currentCooldown + " " + chosenSpells.IndexOf(chosenSpells[i]));
                }
                else
                {
                    hotBars[i].transform.GetChild(1).GetComponent<Image>().fillAmount = 0f;
                }
            }
            
        }

        if (selectedSpell.hasActiveTime && selectedSpell.currentActiveTime < selectedSpell.activeTime) //TODO: rework activetime so that you can scroll, but still have to cancel in order to shoot next spell
        {
            hotBars[hotBarSelected].transform.GetChild(0).GetComponent<Image>().color = Color.green;

            //selectedSpell.longActive(parent_Player, firePoint);
            selectedSpell.currentActiveTime += Time.deltaTime;
            return;
            //cant scroll 
        }
        else
        {
            //called every frame :/
            hotBars[hotBarSelected].transform.GetChild(0).GetComponent<Image>().color = Color.white;
        }
        #endregion

        #region choose hotbar selected
        if (Input.mouseScrollDelta.y != 0)
        {
            hotBarSelected -= (int)Input.mouseScrollDelta.y;
            if (hotBarSelected >= hotBarSlots)
            {
                hotBarSelected = 0;
            }
            else if(hotBarSelected < 0)
            {
                hotBarSelected = (hotBarSlots - 1);
            }
            updateUI_selected();
            return;
        }
        else
        {
            for (int i = 0; i < keyCodes.Length; i++)
            {
                if (Input.GetKeyDown(keyCodes[i]))
                {
                    if (i < hotBarSlots)
                    {
                        hotBarSelected = i;
                        updateUI_selected();
                        return;
                    }
                }
            }
        }
        
            #endregion
    }

    private IEnumerator castSpell(spell spell)
    {
        castSpellUI.startCastVisuals(spell.castTime, spell.name, spell.UI_color);
        spell.preCastTime_Activate(parent_Player, firePoint); // used for charge-effects?
        spell.currentActiveTime = -spell.castTime;

        yield return new WaitForSeconds(spell.castTime);

        spell.Activate(parent_Player, firePoint);
        spell.currentCooldown = 0;
        spell.currentActiveTime = 0;
        spell.inQueue = false;

        if (spell.hasActiveTime)
        {
            foreach(spell e_spell in queuedSpells)
            {
                e_spell.inQueue = false;
            }
            queuedSpells.Clear(); //cant queue spells after a spell that has active-time
        }
        endCastSpell();
    }
    private void endCastSpell()
    {
        if (queuedSpells.Count != 0)
        {
            spell nextSpell = queuedSpells[0];
            queuedSpells.RemoveAt(0);
            currentlyCasting_coroutine = castSpell(nextSpell);
            StartCoroutine(currentlyCasting_coroutine);
        }
        else
        {
            currently_casting = false;
        }
    }

    private void updateUI_selected()
    {
        for(var i = 0; i < hotBars.Length; i++)
        {
            if(i == hotBarSelected)
            {
                hotBars[i].GetComponent<Image>().color = Color.green;
            }
            else
            {
                hotBars[i].GetComponent<Image>().color = Color.white;
            }
        }
    }

    public void setSpell(spell whatSpell)
    {
        for(var i = 0; i < chosenSpells.Count; i++)
        {
            if(chosenSpells[i].name == whatSpell.name) //if you already have this spell in another slot
            {
                chosenSpells[i] = emptySpell_filler;
                hotBars[i].transform.GetChild(0).GetComponent<Image>().sprite = emptySpell_Sprite;
            }
        }

        chosenSpells[hotBarSelected] = whatSpell;
        hotBars[hotBarSelected].transform.GetChild(0).GetComponent<Image>().sprite = whatSpell.spellIcon;
        chosenSpells[hotBarSelected].resetSpellValues();
    }

}
