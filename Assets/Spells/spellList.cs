using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class spellList : MonoBehaviour
{
    public static spellList Instance;

    [Serializable]
    public struct NamedImage
    {
        public spell spell_script;
        public bool banned;
        public bool owned;
    }
    public NamedImage[] allSpellList;

    private void Start()
    {
        if(Instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        
    }
}
