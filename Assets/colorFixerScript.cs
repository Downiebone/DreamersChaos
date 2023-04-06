using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class colorFixerScript : MonoBehaviour
{
    // Start is called before the first frame update
    public Color[] myColors = new Color[9];
    public Image currSelectedColorThing;
    public GameObject colorPickerObject;

    public FlexibleColorPicker colorPicker;

    private void OpenColorPicker()
    {
        colorPickerObject.SetActive(true);
    }
    public void closeColorPicker()
    {
        currSelectedColorThing.color = colorPicker.color;
        myColors[currSelectedColorThing.transform.GetSiblingIndex()] = colorPicker.color;
        colorPickerObject.SetActive(false);
    }
    public void SetCurrSelectedColorThing(Image go)
    {
        currSelectedColorThing = go;
        OpenColorPicker();
    }
}
