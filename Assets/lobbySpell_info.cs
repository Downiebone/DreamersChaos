using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class lobbySpell_info : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string[] tags;
    public string spellName;
    public string spellDescript;
    public bool banned;
    [Space]
    public Image iconImage;
    public Image iconHolderImage;

    public Color bannedColor;

    public bool is_banned = false;

    public void setEverything(spell spellinf, bool banned)
    {
        iconImage.sprite = spellinf.spellIcon;
        spellName = spellinf.name;
        spellDescript = spellinf.description;
        tags = spellinf.searchTags;

        is_banned = !banned;

        updateUI();
    }


    public void updateUI()
    {
        is_banned = !is_banned;

        if (is_banned)
        {
            var tempColor = iconImage.color;
            tempColor.a = 0.7f;
            iconImage.color = tempColor;

            iconHolderImage.color = bannedColor;
        }
        else
        {
            var tempColor = iconImage.color;
            tempColor.a = 1f;
            iconImage.color = tempColor;

            iconHolderImage.color = Color.white;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipObject.show(iconImage.sprite, spellName, spellDescript);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipObject.hide();
    }
}
