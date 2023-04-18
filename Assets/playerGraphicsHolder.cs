using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerGraphicsHolder : MonoBehaviour
{
    public List<GameObject> wandList = new List<GameObject>();
    public List<GameObject> characterParts = new List<GameObject>();
    void Start()
    {
        
    }

    public void enableCharacter(bool on)
    {
        foreach(GameObject ob in characterParts)
        {
            ob.SetActive(on);
        }
    }

    public void layerCharacter()
    {

    }
}
