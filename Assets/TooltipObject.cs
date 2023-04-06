using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TooltipObject : MonoBehaviour
{
    // Start is called before the first frame update
    public static TooltipObject ToolTip;

    public TMP_Text name;
    public Image icon;
    public TMP_Text descript;
    private void Awake()
    {
        ToolTip = this;
        gameObject.SetActive(false);
    }

    public static void show(Sprite sprit, string nam, string descrip)
    {
        ToolTip.transform.parent.position = Input.mousePosition;
        ToolTip.name.text = nam;
        ToolTip.icon.sprite = sprit;
        ToolTip.descript.text = descrip;
        ToolTip.gameObject.SetActive(true);
    }
    public static void hide()
    {
        ToolTip.gameObject.SetActive(false);
    }

    private void Update()
    {
        transform.parent.position = Input.mousePosition;
    }
}
