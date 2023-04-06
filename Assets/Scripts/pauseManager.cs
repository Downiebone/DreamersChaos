using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pauseManager : MonoBehaviour
{
    public bool isPaused = false;
    [SerializeField] private GameObject puseMenu;
    [SerializeField] private GameObject[] thingsThatCanBeEnabled;

    [SerializeField] private GameObject hostSide, controllsSide, AudioSide;

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) { return; }

        //if you press escape
        bool randoEnabled = false;
        foreach(var item in thingsThatCanBeEnabled)
        {
            if (item.activeInHierarchy)
            {
                randoEnabled = true;
                break;
            }
        }

        if (randoEnabled == true) // if in spell screen
        {
            foreach (var item in thingsThatCanBeEnabled)
            {
                item.SetActive(false);
            }
            isPaused = false;
            Cursor.lockState = CursorLockMode.Locked;
            return;
        }
        else
        {
            puseMenu.gameObject.SetActive(true);
            isPaused = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

    }

    public void resumeButton()
    {
        puseMenu.gameObject.SetActive(false);
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        
    }


    public void BTN_hostSide()
    {
        hostSide.SetActive(true);

        controllsSide.SetActive(false);
        AudioSide.SetActive(false);
    }
    public void BTN_controllsSide()
    {
        controllsSide.SetActive(true);

        hostSide.SetActive(false);
        AudioSide.SetActive(false);
    }
    public void BTN_audioSide()
    {
        AudioSide.SetActive(true);

        hostSide.SetActive(false);
        controllsSide.SetActive(false);
    }
}
