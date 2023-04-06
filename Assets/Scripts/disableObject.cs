using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class disableObject : MonoBehaviour
{
    [SerializeField] private GameObject ob;
    [SerializeField] private float duration;
    private void Awake()
    {
        Invoke("dis", duration);   
    }

    private void dis()
    {
        ob.SetActive(false);
    }
}
