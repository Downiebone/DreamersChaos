using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using SerializableTypes;
using UnityEngine.SceneManagement;
using System;
using System.Threading;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class WorldGenerator : MonoBehaviour {
    [HideInInspector] public static WorldGenerator Instance = null;

    [Header("load from file")]
    [SerializeField] private bool loadFromFile = false;
    [SerializeField] private string fileName = "";

    [HideInInspector] public bool hasStarted = false;

    [Header("generate new")]
    public int WorldSizeInChunks = 10;
    [Space]
    public Color[] theColorsOfTheWorld;
    public Material worldMaterial;
    [Space]
    public GameObject bounds;

    [Header("UI")]
    public GameObject UIimage; //prefab
    public GameObject UI_color_pointer;
    public GameObject UIimageContaioner;
    public TMP_Text boxEditText;
    public TMP_Text buildingModeText;

    [Header("startUI")]
    public TMP_InputField inputField_fileName;
    public TMP_InputField inputField_chunks;
    public TMP_Text numChunksText;
    public GameObject startCreate;
    [SerializeField] private colorFixerScript colorChooserThing;
    private List<GameObject> uiColors = new List<GameObject>();

    Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();

    private PlayerMovement playerToInit;
    private setUpOwnerPlayer setupPlayer;

    Byte[] LoadedvectorTextAsset;
    Byte[] LoadedchunkTextAsset;
    Byte[] LoadedcolorTextAsset;

    private bool inEditorScene = false;

    public int playerReadyCount = 1;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        if (SceneManager.GetActiveScene().name == "marchScene")
        {
            inEditorScene = true;
        }
    }
    public void StartGameReal(PlayerMovement playerScriptCallback, setUpOwnerPlayer setup)
    {
        if (!inEditorScene)
        {
            while (transform.childCount != 0)
            {
                Destroy(this.transform.GetChild(0).gameObject);
            }

            playerToInit = playerScriptCallback;
            setupPlayer = setup;
            playerToInit.initPlayerMovement();
            loadChunksStuffAsync();

        }
    }

    public async void loadChunksStuffAsync()
    {
        // Suspends GetDotNetCount() to allow the caller (the web server)
        // to accept another request, rather than blocking on this one.

        //preload chunks (it needs to be done on the main thread :( )

        if (!inEditorScene)
        {
            string path = "MarchingCubes/" + fileName;
            LoadedvectorTextAsset = (Resources.Load(path + "/vectors") as TextAsset).bytes;
            LoadedchunkTextAsset = (Resources.Load(path + "/chunks") as TextAsset).bytes;
            LoadedcolorTextAsset = (Resources.Load(path + "/colors") as TextAsset).bytes;
        }
        await Task.Run(() =>
        {
            LoadChunksFromJson(fileName);
        });
        setupPlayer.setReadyForGame();
        


        
    }

    public void waitingForPlayersToBeReady()
    {
        Debug.Log("WAITING FOR PLAYERS: " + playerReadyCount + "/" + PlayerPrefs.GetInt("PLAYER_COUNT"));
        if (playerReadyCount >= PlayerPrefs.GetInt("PLAYER_COUNT")) //TODO: are all players ready?
        {
            //tell all players that its ok to init
            //playerSetup.
            setupPlayer.tellClientToLoad();
        }
        else
        {
            Invoke("waitingForPlayersToBeReady", 1); //else wait a bit and check again
        }
    }

    public void loadInChunks()
    {
        StartCoroutine(startLoadChunks());
    }
    IEnumerator startLoadChunks()
    {
        yield return null;
        reloadChunks();
        Debug.Log("init Time");
        onLoadedWorldDone();
    }

    private void onLoadedWorldDone()
    {
        //TODO player should already be initid (and walking around in the lil wait room) so here we should just put the player at a spawnpoint
        playerToInit.initPlayerGame();
    }

    public void BeginTheProcess() {
        while (transform.childCount != 0)
        {
            Destroy(this.transform.GetChild(0).gameObject);
        }

        if (loadFromFile == true)
        {
            LoadChunksFromJson(fileName);
            reloadChunks();
        }
        else
        {
            Generate();
        }

        hasStarted = true;

        if (!inEditorScene)
            return;

        bounds.transform.position = Vector3.zero + new Vector3((WorldSizeInChunks * GameData.ChunkWidth) / 2, GameData.ChunkHeight / 2, (WorldSizeInChunks * GameData.ChunkWidth) / 2);
        bounds.transform.localScale = new Vector3((WorldSizeInChunks * GameData.ChunkWidth), GameData.ChunkHeight, (WorldSizeInChunks * GameData.ChunkWidth));

        foreach(Color col in theColorsOfTheWorld)
        {
            GameObject GO = Instantiate(UIimage, Vector3.zero, Quaternion.identity);
            GO.GetComponent<Image>().color = col;
            GO.transform.parent = UIimageContaioner.transform;
            uiColors.Add(GO);
        }

        UI_color_pointer.transform.SetParent(uiColors[0].transform);
        UI_color_pointer.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -20, 0);
        UI_color_pointer.GetComponent<Image>().color = theColorsOfTheWorld[0];
    }

    public void curColorIndex(int ind)
    {
        UI_color_pointer.transform.SetParent(uiColors[ind].transform);
        UI_color_pointer.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -20, 0);
        UI_color_pointer.GetComponent<Image>().color = theColorsOfTheWorld[ind];
    }
    public void setBoxEditText(string axis)
    {
        if (string.IsNullOrWhiteSpace(axis))
        {
            boxEditText.gameObject.SetActive(false);
        }
        else
        {
            boxEditText.gameObject.SetActive(true);
            boxEditText.text = "Axis: " + axis;
        }

    }
    public void setBuildingModeText(GameData.modificationType mode)
    {
        if (mode == GameData.modificationType.Both)
            buildingModeText.color = new Color(100f/255f, 255f/255f, 192f/255f);
        else if (mode == GameData.modificationType.OnlyAir)
            buildingModeText.color = new Color(255f / 255f, 132f / 255f, 223f / 255f);
        else if (mode == GameData.modificationType.OnlyOtherBlocks)
            buildingModeText.color = new Color(100f / 155f, 225f / 255f, 155f / 255f);
        buildingModeText.text = "BuildingMode:" + mode.ToString();
    }
    public void restartALL()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void setSaveFileText()
    {
        string field = inputField_fileName.text;
        fileName = field;
    }
    public void setWorldChunks()
    {
        bool isNumeric = int.TryParse(inputField_chunks.text, out int n);
        if (isNumeric == false)
            return;

        numChunksText.text = n + "*" + n + " chunks";
        int field = n;
        WorldSizeInChunks = field;
    }
    public void startGameFromFile()
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return;

        loadFromFile = true;
        BeginTheProcess();
        Cursor.lockState = CursorLockMode.Locked;
        startCreate.gameObject.SetActive(false);
    }
    public void startGameNew()
    {
        if (WorldSizeInChunks <= 0)
            return;

        loadFromFile = false;
        theColorsOfTheWorld = colorChooserThing.myColors;
        BeginTheProcess();
        Cursor.lockState = CursorLockMode.Locked;
        startCreate.gameObject.SetActive(false);
    }
    public void ModifyChunkFastGameExplosion(Vector3 modificationPoint, float range)
    {

        //Chunk voxel position (based on the chunk system)
        Vector3 vertexOrigin = new Vector3((int)modificationPoint.x, (int)modificationPoint.y, (int)modificationPoint.z);

        //intRange (convert Vector3 real world range to the voxel size range)
        int intRange = (int)(range / 2);//range /2 because the for is from -intRange to +intRange

        List<Vector3Int> toBeModifiedChunks = new List<Vector3Int>();
        int worldLimit = ((WorldSizeInChunks - 1) * GameData.ChunkWidth);
        for (int y = -intRange; y <= intRange; y++)
        {
            for (int z = -intRange; z <= intRange; z++)
            {
                for (int x = -intRange; x <= intRange; x++)
                {
                    if (vertexOrigin.y + y < 0 || vertexOrigin.y + y > GameData.ChunkHeight)
                        continue; // skip points above or below bounds
                    //Avoid edit the first and last height vertex of the chunk, for avoid non-faces in that heights
                    //if (vertexOrigin.y + y >= GameData.ChunkHeight / 2 || vertexOrigin.y + y <= -GameData.ChunkHeight / 2)
                    //    continue;

                    //Edit vertex of the chunk
                    Vector3 vertexPoint = new Vector3(vertexOrigin.x + x, vertexOrigin.y + y, vertexOrigin.z + z);
                    

                    float distance = Vector3.Distance(vertexPoint, modificationPoint);
                    distance += Mathf.Epsilon;
                    if (distance > intRange)//Not in range of modification, we check other vertexs
                    {
                        //Debug.Log("no Rango: "+ distance + " > " + range+ " |  "+ vertexPoint +" / " + modificationPoint);
                        continue;
                    }

                    //Chunk of the vertexPoint
                    Vector3Int hitChunk = new Vector3Int(Mathf.FloorToInt(vertexPoint.x / GameData.ChunkWidth) * GameData.ChunkWidth,
                                                    0,
                                                    Mathf.FloorToInt(vertexPoint.z / GameData.ChunkWidth) * GameData.ChunkWidth);

                    if (hitChunk.x < 0 || hitChunk.z < 0 || hitChunk.x > worldLimit || hitChunk.z > worldLimit) //outside of map
                        continue;

                    toBeModifiedChunks.Add(hitChunk);

                    Vector3Int vertexChunk = new Vector3Int((int)vertexPoint.x, (int)vertexPoint.y, (int)vertexPoint.z);
                    vertexChunk -= hitChunk;

                    //Debug.Log( vertexPoint + " | chunk: "+ hitChunk+ " / " + vertexChunk);//Debug Vertex point to chunk and vertexChunk
                    chunks[hitChunk].changeTerrainMapInGame(vertexChunk);


                    if (hitChunk.x != 0 && hitChunk.z != 0 && vertexChunk.x == 0 && vertexChunk.z == 0)//Interact with chunk(-1,-1), chunk(-1,0) and chunk(0,-1)
                    {

                        //Vertex of chunk (-1,0)
                        hitChunk.x -= GameData.ChunkWidth;//Chunk -1
                        vertexChunk.x = GameData.ChunkWidth; //Vertex of a chunk -1, last vertex
                        chunks[hitChunk].changeTerrainMapInGame(vertexChunk);
                        toBeModifiedChunks.Add(hitChunk);
                        //Vertex of chunk (-1,-1)
                        hitChunk.z -= GameData.ChunkWidth;
                        vertexChunk.z = GameData.ChunkWidth;
                        chunks[hitChunk].changeTerrainMapInGame(vertexChunk);
                        toBeModifiedChunks.Add(hitChunk);
                        //Vertex of chunk (0,-1)
                        hitChunk.x += GameData.ChunkWidth;
                        vertexChunk.x = 0;
                        chunks[hitChunk].changeTerrainMapInGame(vertexChunk);
                        toBeModifiedChunks.Add(hitChunk);
                    }
                    else if (hitChunk.x != 0 && vertexChunk.x == 0)//Interact with vertex of chunk(-1,0)
                    {

                        hitChunk.x -= GameData.ChunkWidth;
                        vertexChunk.x = GameData.ChunkWidth;
                        chunks[hitChunk].changeTerrainMapInGame(vertexChunk);
                        toBeModifiedChunks.Add(hitChunk);
                    }
                    else if (hitChunk.z != 0 && vertexChunk.z == 0)//Interact with vertex of chunk(0,-1)
                    {

                        hitChunk.z -= GameData.ChunkWidth;
                        vertexChunk.z = GameData.ChunkWidth;
                        chunks[hitChunk].changeTerrainMapInGame(vertexChunk);
                        toBeModifiedChunks.Add(hitChunk);
                    }
                }
            }
        }

        foreach (Vector3Int v3int in toBeModifiedChunks.Distinct().ToList())
        {
            if (modificationPoint.y + range > chunks[v3int].curMaxHeight)
            {
                int newHigh = Mathf.CeilToInt(modificationPoint.y + range + 1);
                if (newHigh > GameData.ChunkHeight)
                    newHigh = GameData.ChunkHeight;
                chunks[v3int].curMaxHeight = newHigh;

            }

            chunks[v3int].CreateMeshData();
        }
    }

    public void ModifyChunkDataSpher(Vector3 modificationPoint, float range, int modification, int mat, GameData.modificationType modType)
    {

        //Chunk voxel position (based on the chunk system)
        Vector3 vertexOrigin = new Vector3((int)modificationPoint.x, (int)modificationPoint.y, (int)modificationPoint.z);

        //intRange (convert Vector3 real world range to the voxel size range)
        int intRange = (int)(range / 2);//range /2 because the for is from -intRange to +intRange

        List<Vector3Int> toBeModifiedChunks = new List<Vector3Int>();
        int worldLimit = ((WorldSizeInChunks - 1) * GameData.ChunkWidth);
        for (int y = -intRange; y <= intRange; y++)
        {
            for (int z = -intRange; z <= intRange; z++)
            {
                for (int x = -intRange; x <= intRange; x++)
                {
                    if (vertexOrigin.y + y < 0 || vertexOrigin.y + y > GameData.ChunkHeight)
                        continue; // skip points above or below bounds
                    //Avoid edit the first and last height vertex of the chunk, for avoid non-faces in that heights
                    //if (vertexOrigin.y + y >= GameData.ChunkHeight / 2 || vertexOrigin.y + y <= -GameData.ChunkHeight / 2)
                    //    continue;

                    //Edit vertex of the chunk
                    Vector3 vertexPoint = new Vector3(vertexOrigin.x + x, vertexOrigin.y + y, vertexOrigin.z + z);

                    float distance = Vector3.Distance(vertexPoint, modificationPoint);
                    distance += Mathf.Epsilon;
                    if (distance > intRange)//Not in range of modification, we check other vertexs
                    {
                        //Debug.Log("no Rango: "+ distance + " > " + range+ " |  "+ vertexPoint +" / " + modificationPoint);
                        continue;
                    }

                    //Chunk of the vertexPoint
                    Vector3Int hitChunk = new Vector3Int(Mathf.FloorToInt(vertexPoint.x / GameData.ChunkWidth) * GameData.ChunkWidth,
                                                    0,
                                                    Mathf.FloorToInt(vertexPoint.z / GameData.ChunkWidth) * GameData.ChunkWidth);
                    
                    if (hitChunk.x < 0 || hitChunk.z < 0 || hitChunk.x > worldLimit || hitChunk.z > worldLimit) //outside of map
                        continue;

                    toBeModifiedChunks.Add(hitChunk);

                    Vector3Int vertexChunk = new Vector3Int((int)vertexPoint.x,(int)vertexPoint.y,(int)vertexPoint.z);
                    vertexChunk -= hitChunk;

                    //Debug.Log( vertexPoint + " | chunk: "+ hitChunk+ " / " + vertexChunk);//Debug Vertex point to chunk and vertexChunk
                    chunks[hitChunk].changeTerrainMap(vertexChunk, modification, mat, modType);


                    if (hitChunk.x != 0 && hitChunk.z != 0 && vertexChunk.x == 0 && vertexChunk.z == 0)//Interact with chunk(-1,-1), chunk(-1,0) and chunk(0,-1)
                    {

                        //Vertex of chunk (-1,0)
                        hitChunk.x -= GameData.ChunkWidth;//Chunk -1
                        vertexChunk.x = GameData.ChunkWidth; //Vertex of a chunk -1, last vertex
                        chunks[hitChunk].changeTerrainMap(vertexChunk, modification, mat, modType);
                        toBeModifiedChunks.Add(hitChunk);
                        //Vertex of chunk (-1,-1)
                        hitChunk.z -= GameData.ChunkWidth;
                        vertexChunk.z = GameData.ChunkWidth;
                        chunks[hitChunk].changeTerrainMap(vertexChunk, modification, mat, modType);
                        toBeModifiedChunks.Add(hitChunk);
                        //Vertex of chunk (0,-1)
                        hitChunk.x += GameData.ChunkWidth;
                        vertexChunk.x = 0;
                        chunks[hitChunk].changeTerrainMap(vertexChunk, modification, mat, modType);
                        toBeModifiedChunks.Add(hitChunk);
                    }
                    else if (hitChunk.x != 0 && vertexChunk.x == 0)//Interact with vertex of chunk(-1,0)
                    {

                        hitChunk.x -= GameData.ChunkWidth;
                        vertexChunk.x = GameData.ChunkWidth;
                        chunks[hitChunk].changeTerrainMap(vertexChunk, modification, mat, modType);
                        toBeModifiedChunks.Add(hitChunk);
                    }
                    else if (hitChunk.z != 0 && vertexChunk.z == 0)//Interact with vertex of chunk(0,-1)
                    {

                        hitChunk.z -= GameData.ChunkWidth;
                        vertexChunk.z = GameData.ChunkWidth;
                        chunks[hitChunk].changeTerrainMap(vertexChunk, modification, mat, modType);
                        toBeModifiedChunks.Add(hitChunk);
                    }
                }
            }
        }

        foreach(Vector3Int v3int in toBeModifiedChunks.Distinct().ToList())
        {
            if (modificationPoint.y + range > chunks[v3int].curMaxHeight)
            {
                int newHigh = Mathf.CeilToInt(modificationPoint.y + range + 1);
                if (newHigh > GameData.ChunkHeight)
                    newHigh = GameData.ChunkHeight;
                chunks[v3int].curMaxHeight = newHigh;

            }
                
            chunks[v3int].CreateMeshData();
        }
    }

    public void ModifyChunkDataCube(Vector3 modificationPoint, Vector3Int area, int modification, int mat, GameData.modificationType modType)
    {

        //Chunk voxel position (based on the chunk system)
        Vector3 vertexOrigin = new Vector3((int)modificationPoint.x, (int)modificationPoint.y, (int)modificationPoint.z);

        //intRange (convert Vector3 real world range to the voxel size range)
        //int intRange = (int)(range / 2);//range /2 because the for is from -intRange to +intRange

        List<Vector3Int> toBeModifiedChunks = new List<Vector3Int>();
        int worldLimit = ((WorldSizeInChunks - 1) * GameData.ChunkWidth);
        for (int y = -area.y; y <= area.y; y++)
        {
            for (int z = -area.z; z <= area.z; z++)
            {
                for (int x = -area.x; x <= area.x; x++)
                {
                    if (vertexOrigin.y + y < 0 || vertexOrigin.y + y > GameData.ChunkHeight)
                        continue; // skip points above or below bounds
                    //Avoid edit the first and last height vertex of the chunk, for avoid non-faces in that heights
                    //if (vertexOrigin.y + y >= GameData.ChunkHeight / 2 || vertexOrigin.y + y <= -GameData.ChunkHeight / 2)
                    //    continue;

                    //Edit vertex of the chunk
                    Vector3 vertexPoint = new Vector3(vertexOrigin.x + x, vertexOrigin.y + y, vertexOrigin.z + z);

                    //Chunk of the vertexPoint
                    Vector3Int hitChunk = new Vector3Int(Mathf.FloorToInt(vertexPoint.x / GameData.ChunkWidth) * GameData.ChunkWidth,
                                                    0,
                                                    Mathf.FloorToInt(vertexPoint.z / GameData.ChunkWidth) * GameData.ChunkWidth);

                    if (hitChunk.x < 0 || hitChunk.z < 0 || hitChunk.x > worldLimit || hitChunk.z > worldLimit) //outside of map
                        continue;

                    toBeModifiedChunks.Add(hitChunk);

                    Vector3Int vertexChunk = new Vector3Int((int)vertexPoint.x, (int)vertexPoint.y, (int)vertexPoint.z);
                    vertexChunk -= hitChunk;

                    //Debug.Log( vertexPoint + " | chunk: "+ hitChunk+ " / " + vertexChunk);//Debug Vertex point to chunk and vertexChunk
                    chunks[hitChunk].changeTerrainMap(vertexChunk, modification, mat, modType);


                    if (hitChunk.x != 0 && hitChunk.z != 0 && vertexChunk.x == 0 && vertexChunk.z == 0)//Interact with chunk(-1,-1), chunk(-1,0) and chunk(0,-1)
                    {

                        //Vertex of chunk (-1,0)
                        hitChunk.x -= GameData.ChunkWidth;//Chunk -1
                        vertexChunk.x = GameData.ChunkWidth; //Vertex of a chunk -1, last vertex
                        chunks[hitChunk].changeTerrainMap(vertexChunk, modification, mat, modType);
                        toBeModifiedChunks.Add(hitChunk);
                        //Vertex of chunk (-1,-1)
                        hitChunk.z -= GameData.ChunkWidth;
                        vertexChunk.z = GameData.ChunkWidth;
                        chunks[hitChunk].changeTerrainMap(vertexChunk, modification, mat, modType);
                        toBeModifiedChunks.Add(hitChunk);
                        //Vertex of chunk (0,-1)
                        hitChunk.x += GameData.ChunkWidth;
                        vertexChunk.x = 0;
                        chunks[hitChunk].changeTerrainMap(vertexChunk, modification, mat, modType);
                        toBeModifiedChunks.Add(hitChunk);
                    }
                    else if (hitChunk.x != 0 && vertexChunk.x == 0)//Interact with vertex of chunk(-1,0)
                    {

                        hitChunk.x -= GameData.ChunkWidth;
                        vertexChunk.x = GameData.ChunkWidth;
                        chunks[hitChunk].changeTerrainMap(vertexChunk, modification, mat, modType);
                        toBeModifiedChunks.Add(hitChunk);
                    }
                    else if (hitChunk.z != 0 && vertexChunk.z == 0)//Interact with vertex of chunk(0,-1)
                    {

                        hitChunk.z -= GameData.ChunkWidth;
                        vertexChunk.z = GameData.ChunkWidth;
                        chunks[hitChunk].changeTerrainMap(vertexChunk, modification, mat, modType);
                        toBeModifiedChunks.Add(hitChunk);
                    }
                }
            }
        }

        foreach (Vector3Int v3int in toBeModifiedChunks.Distinct().ToList())
        {
            if (modificationPoint.y + area.y > chunks[v3int].curMaxHeight)
            {
                int newHigh = Mathf.CeilToInt(modificationPoint.y + area.y + 1);
                if (newHigh > GameData.ChunkHeight)
                    newHigh = GameData.ChunkHeight;
                chunks[v3int].curMaxHeight = newHigh;

            }

            chunks[v3int].CreateMeshData();
        }
    }
#if UNITY_EDITOR
    private void deGenerate(PlayModeStateChange state)
    {
        StartCoroutine(deGenerateCo());
    }

    IEnumerator deGenerateCo()
    {
        yield return null;
        while (transform.childCount != 0)
        {
            DestroyImmediate(this.transform.GetChild(0).gameObject);
        }
    }
    IEnumerator tryNextFrame()
    {
        Debug.Log("REDOING");
        yield return null;
        while (transform.childCount != 0)
        {
            DestroyImmediate(this.transform.GetChild(0).gameObject);
        }
        yield return null;
        LoadData();
    }
#endif
    void Generate () {
        //loadingScreen.SetActive(true);
        for (int x = 0; x < WorldSizeInChunks; x++) {
            for (int z = 0; z < WorldSizeInChunks; z++) {

                Vector3Int chunkPos = new Vector3Int(x * GameData.ChunkWidth, 0, z * GameData.ChunkWidth);
                chunks.Add(chunkPos, new Chunk(chunkPos,this));
                chunks[chunkPos].chunkObject.transform.SetParent(transform);

            }
        }
        //loadingScreen.SetActive(false);
        Debug.Log(string.Format("{0} x {0} world generated.", (WorldSizeInChunks * GameData.ChunkWidth)));
    }
    void reloadChunks()
    {
        for (int x = 0; x < WorldSizeInChunks; x++)
        {
            for (int z = 0; z < WorldSizeInChunks; z++)
            {

                Vector3Int chunkPos = new Vector3Int(x * GameData.ChunkWidth, 0, z * GameData.ChunkWidth);
                chunks[chunkPos].createFromData(chunkPos, this);
                chunks[chunkPos].chunkObject.transform.SetParent(transform);
            }
        }
    }
#if UNITY_EDITOR
    void EditorGenerate()
    {
        //loadingScreen.SetActive(true);
        for (int x = 0; x < WorldSizeInChunks; x++)
        {
            for (int z = 0; z < WorldSizeInChunks; z++)
            {

                Vector3Int chunkPos = new Vector3Int(x * GameData.ChunkWidth, 0, z * GameData.ChunkWidth);
                Chunk tempChunk = new Chunk(chunkPos, this);
                tempChunk.chunkObject.transform.SetParent(transform);

            }
        }
        Debug.Log("chunks:" + chunks.Keys.Count);
        //loadingScreen.SetActive(false);
        Debug.Log(string.Format("{0} x {0} world generated.", (WorldSizeInChunks * GameData.ChunkWidth)));
    }
    void editorReloadChunks()
    {
        for (int x = 0; x < WorldSizeInChunks; x++)
        {
            for (int z = 0; z < WorldSizeInChunks; z++)
            {

                Vector3Int chunkPos = new Vector3Int(x * GameData.ChunkWidth, 0, z * GameData.ChunkWidth);
                chunks[chunkPos].createFromData(chunkPos, this);
                chunks[chunkPos].chunkObject.transform.SetParent(transform);
            }
        }
    }
#endif

    public byte[] Serializer(object _object)
    {
        byte[] bytes;
        using (var _MemoryStream = new MemoryStream())
        {
            IFormatter _BinaryFormatter = new BinaryFormatter();
            _BinaryFormatter.Serialize(_MemoryStream, _object);
            bytes = _MemoryStream.ToArray();
        }
        return bytes;
    }

    public T Deserializer<T>(byte[] _byteArray)
    {
        T ReturnValue;
        using (var _MemoryStream = new MemoryStream(_byteArray))
        {
            IFormatter _BinaryFormatter = new BinaryFormatter();
            ReturnValue = (T)_BinaryFormatter.Deserialize(_MemoryStream);
        }
        return ReturnValue;
    }

    public void SaveChunksToJson(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return;


#if UNITY_EDITOR
        string path = "Assets/Resources/MarchingCubes/" + fileName;
#elif UNITY_STANDALONE
        string path = Application.persistentDataPath + "/MarchingCubesSaves/" + fileName;
#endif
        Debug.Log("SAVING TO " + path);
        if (!File.Exists(path + "/chunks.bytes"))
        {
            Directory.CreateDirectory(path);
            
        }
        else
        {
            Debug.LogError("FILE ALREADY EXISTS, OVERRIDING OLD SAVE");
        }
        

        List<Vector3Int> keyList = chunks.Keys.ToList();
        List<SVector3Int> sKeyList = new List<SVector3Int>();
        for(var i = 0; i < keyList.Count; i++)
        {
            sKeyList.Add(new SVector3Int(keyList[i]));
        }


        List<Chunk> chunkList = chunks.Values.ToList();

        foreach(Chunk chunk in chunkList)
        {
            chunk.convertTerrainMapIntoSerializealbe();
        }

        byte[] vectorByte = Serializer(sKeyList);
        Debug.Log("saving vectorsString: " + vectorByte.Length + " " + keyList.Count);
        byte[] chunkByte = Serializer(chunkList);
        Debug.Log("saving chunksString: " + chunkByte.Length + " " + chunkList.Count);




        File.WriteAllBytes(path + "/vectors.bytes", vectorByte);
        File.WriteAllBytes(path + "/chunks.bytes", chunkByte);

        SColor[] sColorsOfTheWorld = new SColor[theColorsOfTheWorld.Length];
        for(var i = 0; i < theColorsOfTheWorld.Length; i++)
        {
            sColorsOfTheWorld[i] = new SColor(theColorsOfTheWorld[i]);
        }

        byte[] colorByte = Serializer(sColorsOfTheWorld);
        Debug.Log("saving colorsString: " + colorByte.Length);

        File.WriteAllBytes(path + "/colors.bytes", colorByte);

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif

    }
    public void LoadChunksFromJson(string fileName)
    {
#if UNITY_EDITOR
        string path = "MarchingCubes/" + fileName;
        loadResourcesFile(path);
#elif UNITY_STANDALONE //TODO: MAKE IT LOAD FROM RESOURCES!! maybe: https://answers.unity.com/questions/8187/how-can-i-read-binary-files-from-resources.html

        if (!inEditorScene)
        {
            string path = "MarchingCubes/" + fileName;
            loadResourcesFile(path);
        }
        else
        {
            string path = Application.persistentDataPath + "/MarchingCubesSaves/" + fileName;
            loadLocalFile(path);
        }
#endif
    }

    public void loadLocalFile(string path)
    {
        Resources.Load(path);
        Debug.Log("LOADING LOCAL FROM " + path);

        

        if (!File.Exists(path + "/chunks.bytes"))
        {
            Debug.LogError("FILE DOEST NOT EXIST, CANT LOAD");
            return;
        }
        byte[] vectorString = File.ReadAllBytes(path + "/vectors.bytes");
        byte[] chunkString = File.ReadAllBytes(path + "/chunks.bytes");
        Debug.Log("loading vectorsString: " + vectorString.Length);
        Debug.Log("loading chunksString: " + chunkString.Length);
        List<SVector3Int> keyList = Deserializer<List<SVector3Int>>(vectorString);
        List<Vector3Int> sKeyList = new List<Vector3Int>();
        for (var i = 0; i < keyList.Count; i++)
        {
            sKeyList.Add(keyList[i]);
        }
        List<Chunk> chunkList = Deserializer<List<Chunk>>(chunkString);

        Debug.Log("loading: " + Mathf.Sqrt(chunkList.Count).ToString() + " * " + Mathf.Sqrt(chunkList.Count).ToString() + " chunks");

        WorldSizeInChunks = (int)Mathf.Sqrt(chunkList.Count);
        for (var i = 0; i < chunkList.Count; i++)
        {
            chunks.Add(sKeyList[i], chunkList[i]);
        }

        byte[] colorString = File.ReadAllBytes(path + "/colors.bytes");
        SColor[] Snew_ColorsOfTheWorld = Deserializer<SColor[]>(colorString);
        Color[] new_ColorsOfTheWorld = new Color[Snew_ColorsOfTheWorld.Length];
        for (var i = 0; i < Snew_ColorsOfTheWorld.Length; i++)
        {
            new_ColorsOfTheWorld[i] = Snew_ColorsOfTheWorld[i];
        }

        theColorsOfTheWorld = new_ColorsOfTheWorld;
    }
    public void loadResourcesFile(string path)
    {
        Debug.Log("LOADING RESOURCES FROM " + path);
        Debug.Log("Assets/Resources/" + path + "/chunks.bytes");
#if UNITY_EDITOR
        if (!File.Exists("Assets/Resources/" + path + "/chunks.bytes"))
        {
            Debug.LogError("FILE DOEST NOT EXIST, CANT LOAD");
            return;
        }
#endif

        byte[] vectorString;
        byte[] chunkString;
        byte[] colorString;

        if (inEditorScene)
        {
            TextAsset vectorTextAsset = Resources.Load(path + "/vectors") as TextAsset;
            TextAsset chunkTextAsset = Resources.Load(path + "/chunks") as TextAsset;
            TextAsset colorTextAsset = Resources.Load(path + "/colors") as TextAsset;

            Debug.Log("1");
            Debug.Log(vectorTextAsset);
            Debug.Log("2");
            Debug.Log(vectorTextAsset.text);
            Debug.Log("3");
            Debug.Log(vectorTextAsset.bytes);

            vectorString = vectorTextAsset.bytes;
            chunkString = chunkTextAsset.bytes;
            colorString = colorTextAsset.bytes;
        }
        else
        {
            vectorString = LoadedvectorTextAsset;
            chunkString = LoadedchunkTextAsset;
            colorString = LoadedcolorTextAsset;
        }
        

        Debug.Log("loading vectorsString: " + vectorString.Length);
        Debug.Log("loading chunksString: " + chunkString.Length);
        List<SVector3Int> keyList = Deserializer<List<SVector3Int>>(vectorString);
        List<Vector3Int> sKeyList = new List<Vector3Int>();
        for (var i = 0; i < keyList.Count; i++)
        {
            sKeyList.Add(keyList[i]);
        }
        List<Chunk> chunkList = Deserializer<List<Chunk>>(chunkString);

        Debug.Log("loading: " + Mathf.Sqrt(chunkList.Count).ToString() + " * " + Mathf.Sqrt(chunkList.Count).ToString() + " chunks");

        WorldSizeInChunks = (int)Mathf.Sqrt(chunkList.Count);
        for (var i = 0; i < chunkList.Count; i++)
        {
            chunks.Add(sKeyList[i], chunkList[i]);
        }

        SColor[] Snew_ColorsOfTheWorld = Deserializer<SColor[]>(colorString);
        Color[] new_ColorsOfTheWorld = new Color[Snew_ColorsOfTheWorld.Length];
        for (var i = 0; i < Snew_ColorsOfTheWorld.Length; i++)
        {
            new_ColorsOfTheWorld[i] = Snew_ColorsOfTheWorld[i];
        }

        theColorsOfTheWorld = new_ColorsOfTheWorld;
    }

    public Chunk GetChunkFromVector3 (Vector3 pos) {

        int x = (int)pos.x;
        int y = (int)pos.y;
        int z = (int)pos.z;

        return chunks[new Vector3Int(x, y, z)];

    }
    public bool IsInsideMesh(Vector3 point)
    {
        RaycastHit[] _hitsUp = new RaycastHit[0]; //temp ?
        RaycastHit[] _hitsDown = new RaycastHit[0];
        Physics.queriesHitBackfaces = true;
        int hitsUp = Physics.RaycastNonAlloc(point, Vector3.up, _hitsUp);
        int hitsDown = Physics.RaycastNonAlloc(point, Vector3.down, _hitsDown);
        Physics.queriesHitBackfaces = false;
        for (var i = 0; i < hitsUp; i++)
             if (_hitsUp[i].normal.y > 0)
                 for (var j = 0; j < hitsDown; j++)
                     if (_hitsDown[j].normal.y < 0 && _hitsDown[j].collider == _hitsUp[i].collider)
                         return true;

        return false;
    }

    public void LoadData()
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            if (transform.childCount != 0)
            {
                StartCoroutine(tryNextFrame());
                return;
            }
            //Do stuff only when in editor and not in playmode
            if (loadFromFile == true)
            {
                LoadChunksFromJson(fileName);
                editorReloadChunks();
            }
            else
            {
                EditorGenerate();
            }
            return;
        }
        else
        {
            //Do stuff you would normally do
        }
#endif
    }

    //<<< End of EditorApplication.isPlaying check


    //Declare bool that will be used as a "button", it's best used with preprocessor directive so you don't have this in final build 
#if UNITY_EDITOR
    [Header("Generate")]
    public bool GenerateD = false;
    [Header("DeGenerate")]
    public bool DeGenerateD = false;

    //You can have multiple booleans here
    private void OnValidate()
    {
        if (GenerateD)
        {
            // Your function here
            LoadData();

            //When its done set this bool to false
            //This is useful if you want to do some stuff only when clicking this "button"
            GenerateD = false;
        }
        if (DeGenerateD)
        {
            Debug.Log("1");
            // Your function here
            deGenerate(PlayModeStateChange.ExitingEditMode);
            Debug.Log("2");
            //When its done set this bool to false
            //This is useful if you want to do some stuff only when clicking this "button"
            DeGenerateD = false;
        }
    }
    
#endif

}
