using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destroyAfterTime : MonoBehaviour
{
    [SerializeField] private float time = 1;
    private void Awake()
    {
        Destroy(this.gameObject, time);
    }
}
