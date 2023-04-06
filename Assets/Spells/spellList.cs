using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class spellList : MonoBehaviour
{
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
        DontDestroyOnLoad(this.gameObject);
    }
}
