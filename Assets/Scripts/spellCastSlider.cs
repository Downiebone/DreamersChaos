using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class spellCastSlider : MonoBehaviour
{
    [SerializeField] private Slider castSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text castName;
    [SerializeField] private GameObject castVisualsHolder;
    private void Start()
    {
        startCastVisuals(0.1f, "_", Color.white);
    }
    public void startCastVisuals(float duration, string spellName, Color castSliderColor)
    {
        if (duration <= 0) //no casttime = no visual bar
            return;

        castSlider.value = 0;
        fillImage.color = castSliderColor;
        castName.color = castSliderColor;
        castName.text = spellName;
        castVisualsHolder.SetActive(true);
        StartCoroutine(SimpleLerp(duration));
    }
    private IEnumerator SimpleLerp(float duration)
    {
        int a = 0;           // start
        int b = 1;           // end
        float x = duration;  // time frame
        float n = 0;         // lerped value
        for (float f = 0; f <= x; f += Time.deltaTime)
        {
            castSlider.value = n;
            n = Mathf.Lerp(a, b, f / x); // passing in the start + end values, and using our elapsed time 'f' as a portion of the total time 'x'
                                         // use 'n' .. ?
            yield return null;
        }
        castSlider.value = 1;
        castVisualsHolder.SetActive(false);
    }
}
