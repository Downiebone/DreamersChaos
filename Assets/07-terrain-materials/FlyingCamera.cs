using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FlyingCamera : MonoBehaviour {

    public Camera cam;
    public WorldGenerator world;
    [SerializeField] private float speed = 10;
    [SerializeField] private bool hold = false;

    public int modificationRadius = 4;

    [SerializeField] private GameObject highlightSphere;
    [SerializeField] private GameObject highlightCube;
    public bool paused = false;
    public GameObject saveMenu;
    public TMP_InputField inputField;
    public string saveFile_textField;

    [SerializeField] private LayerMask mask;

    private int selectedColor = 0;

    private bool editingWithCube = false;
    private int selectedBoxAxisChange = 0;
    private GameData.modificationType buildingMode = GameData.modificationType.Both;

    private KeyCode[] keyCodes = {
         KeyCode.Alpha1,
         KeyCode.Alpha2,
         KeyCode.Alpha3,
         KeyCode.Alpha4,
         KeyCode.Alpha5,
         KeyCode.Alpha6,
         KeyCode.Alpha7,
         KeyCode.Alpha8,
         KeyCode.Alpha9,
     };
    private void Start()
    {
        
        highlightSphere.transform.localScale = new Vector3(modificationRadius, modificationRadius, modificationRadius);

        //set start color
        Color currColor = world.theColorsOfTheWorld[selectedColor];
        currColor.a = 0.6f;

        highlightSphere.GetComponent<MeshRenderer>().material.color = currColor;
        highlightCube.GetComponent<MeshRenderer>().material.color = currColor;

        world.setBuildingModeText(buildingMode);
    }
    public void saveFileToBytes()
    {
        world.SaveChunksToJson(saveFile_textField);
    }
    public void setSaveFileText()
    {
        string field = inputField.text;
        saveFile_textField = field;
    }
    private void Update() {

        if (world.hasStarted == false)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            paused = !paused;
            saveMenu.gameObject.SetActive(paused);

            if(paused)
                Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Locked;
        }
        if (paused)
            return;




        if (Input.GetKeyDown(KeyCode.F))
        {
            editingWithCube = !editingWithCube;
            world.setBoxEditText(editingWithCube == false ? "" : selectedBoxAxisChange == 0 ? "X" : selectedBoxAxisChange == 1 ? "Y" : "Z");
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            modifyChunk(0, transform.position);
        }
            

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            speed *= 3;
        }else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            speed /= 3;
        }

        transform.position = Vector3.MoveTowards(transform.position, transform.position + (cam.transform.forward * Input.GetAxis("Vertical")) + (transform.right * Input.GetAxis("Horizontal")), Time.deltaTime * speed);
        if (Input.GetKey(KeyCode.Space))
            transform.Translate(Vector3.up * speed * Time.deltaTime);
        
        else if (Input.GetKey(KeyCode.LeftShift))
            transform.Translate(-Vector3.up * speed * Time.deltaTime);

        
        
        transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X"), 0));
        cam.transform.Rotate(new Vector3(-Input.GetAxis("Mouse Y"), 0, 0));

        if (Input.GetMouseButtonDown(2) && editingWithCube)
        {
            selectedBoxAxisChange++;
            if (selectedBoxAxisChange > 2)
                selectedBoxAxisChange = 0;
            world.setBoxEditText(selectedBoxAxisChange == 0 ? "X" : selectedBoxAxisChange == 1 ? "Y" : "Z");
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (buildingMode == GameData.modificationType.Both)
                buildingMode = GameData.modificationType.OnlyAir;
            else if (buildingMode == GameData.modificationType.OnlyAir)
                buildingMode = GameData.modificationType.OnlyOtherBlocks;
            else if (buildingMode == GameData.modificationType.OnlyOtherBlocks)
                buildingMode = GameData.modificationType.Both;

            world.setBuildingModeText(buildingMode);
        }

        for (int i = 0; i < keyCodes.Length; i++)
        {
            if (Input.GetKeyDown(keyCodes[i]))
            {
                if (i < world.theColorsOfTheWorld.Length)
                {
                    selectedColor = i;

                    Color currColor = world.theColorsOfTheWorld[selectedColor];
                    world.curColorIndex(selectedColor);
                    currColor.a = 0.6f;

                    highlightSphere.GetComponent<MeshRenderer>().material.color = currColor;
                    highlightCube.GetComponent<MeshRenderer>().material.color = currColor;
                }
            }
        }

        if (Input.mouseScrollDelta.y != 0)
        {
            //color
            if (Input.GetKey(KeyCode.LeftControl))
            {
                selectedColor -= (int)Input.mouseScrollDelta.y;
                if (selectedColor < 0)
                    selectedColor = world.theColorsOfTheWorld.Length - 1;
                else if (selectedColor >= world.theColorsOfTheWorld.Length)
                    selectedColor = 0;

                Color currColor = world.theColorsOfTheWorld[selectedColor];
                world.curColorIndex(selectedColor);
                currColor.a = 0.6f;

                highlightSphere.GetComponent<MeshRenderer>().material.color = currColor;
                highlightCube.GetComponent<MeshRenderer>().material.color = currColor;

            }
            else
            {
                //modification radious
                

                if (editingWithCube)
                {
                    switch (selectedBoxAxisChange)
                    {
                        case 0:
                            if ((highlightCube.transform.localScale.x + (int)Input.mouseScrollDelta.y) < 1)
                                break;
                            highlightCube.transform.localScale = new Vector3(highlightCube.transform.localScale.x + (int)Input.mouseScrollDelta.y, highlightCube.transform.localScale.y, highlightCube.transform.localScale.z);
                            break;
                        case 1:
                            if ((highlightCube.transform.localScale.y + (int)Input.mouseScrollDelta.y) < 1)
                                break;
                            highlightCube.transform.localScale = new Vector3(highlightCube.transform.localScale.x, highlightCube.transform.localScale.y + (int)Input.mouseScrollDelta.y, highlightCube.transform.localScale.z);
                            break;
                        case 2:
                            if ((highlightCube.transform.localScale.z + (int)Input.mouseScrollDelta.y) < 1)
                                break;
                            highlightCube.transform.localScale = new Vector3(highlightCube.transform.localScale.x, highlightCube.transform.localScale.y, highlightCube.transform.localScale.z + (int)Input.mouseScrollDelta.y);
                            break;
                    }
                }
                else
                {
                    modificationRadius += (int)Input.mouseScrollDelta.y;
                    if (modificationRadius < 2)
                        modificationRadius = 2;

                    highlightSphere.transform.localScale = new Vector3(modificationRadius, modificationRadius, modificationRadius);
                }
            }
            
        }


        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1f));
        RaycastHit hit;

        bool hitSome = Physics.Raycast(ray, out hit, Mathf.Infinity, mask);

        highlightSphere.GetComponent<MeshRenderer>().enabled = hitSome && !editingWithCube;
        highlightCube.GetComponent<MeshRenderer>().enabled = hitSome && editingWithCube;
        if (hitSome == false)
            return;
        highlightSphere.transform.position = hit.point;
        highlightCube.transform.position = hit.point;

        

        if (!hold)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (hit.transform.tag == "Terrain")
                {
                    modifyChunk(0, hit.point);
                }
                    
            }

            if (Input.GetMouseButtonDown(1))
            {
                if (hit.transform.tag == "Terrain")
                {
                    modifyChunk(1, hit.point);
                }
            }
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                if (hit.transform.tag == "Terrain")
                {
                    modifyChunk(0, hit.point);
                }
            }

            if (Input.GetMouseButton(1))
            {
                if (hit.transform.tag == "Terrain")
                {
                    modifyChunk(1, hit.point);
                }
            }
        }
            

    }

    private void modifyChunk(int add, Vector3 point)
    {
        Vector3Int highlightCubeInt = new Vector3Int((int)highlightCube.transform.localScale.x / 2, (int)highlightCube.transform.localScale.y / 2, (int)highlightCube.transform.localScale.z / 2);

        if (editingWithCube)
            world.ModifyChunkDataCube(point, highlightCubeInt, add, selectedColor, buildingMode);
        else
            world.ModifyChunkDataSpher(point, modificationRadius, add, selectedColor, buildingMode);
    }

}
