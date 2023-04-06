using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scaleUp : MonoBehaviour
{
    [SerializeField] private float scaleHeight = 1;
    [SerializeField] private float scaleTime = 1;

    private BoxCollider collid;
    private landMine launchPad;
    private Rigidbody rb;
    private void Awake()
    {
        collid = GetComponent<BoxCollider>();
        launchPad = GetComponent<landMine>();
        rb = GetComponent<Rigidbody>();

        transform.localScale = new Vector3(transform.localScale.x, 0, transform.localScale.z);
        StartCoroutine(scaleUpC());
    }
    private IEnumerator scaleUpC()
    {
        float a = 0f;           // start
        float b = scaleHeight;           // end
        float x = scaleTime;  // time frame
        float n = 0;         // lerped value
        for (float f = 0; f <= x; f += Time.deltaTime)
        {
            n = Mathf.Lerp(a, b, f / x); // passing in the start + end values, and using our elapsed time 'f' as a portion of the total time 'x'
            transform.localScale = new Vector3(transform.localScale.x, n, transform.localScale.z);
                                         // use 'n' .. ?
            yield return null;
        }
        launchPad.enabled = false;
        collid.isTrigger = false;
        Destroy(rb);
        transform.localScale = new Vector3(transform.localScale.x, b, transform.localScale.z);
    }
}
