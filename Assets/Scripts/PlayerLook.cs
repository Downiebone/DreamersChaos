using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] WallRun wallRun;

    [SerializeField] private float sensX = 100f;
    [SerializeField] private float sensY = 100f;
    public float sensMultiplier = 1f;

    [SerializeField] Transform cam = null;
    [SerializeField] Transform orientation = null;


    float mouseX;
    float mouseY;

    float multiplier = 0.01f;

    float xRotation;
    float yRotation;

    [HideInInspector] public pauseManager pauseScript;

    private void Start()
    {
        pauseScript = GameObject.FindGameObjectWithTag("pauseManager").GetComponent<pauseManager>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (pauseScript.isPaused) { return; }//dont look if paused
        mouseX = Input.GetAxisRaw("Mouse X");
        mouseY = Input.GetAxisRaw("Mouse Y");

        yRotation += mouseX * sensX * multiplier * sensMultiplier;
        xRotation -= mouseY * sensY * multiplier * sensMultiplier;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cam.transform.rotation = Quaternion.Euler(xRotation, yRotation, wallRun.tilt);

        orientation.transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void changeSens(float newSens)
    {
        sensMultiplier = newSens;
    }
}