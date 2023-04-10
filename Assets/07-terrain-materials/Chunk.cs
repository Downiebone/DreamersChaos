using SerializableTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Chunk {
    [NonSerialized]
    public GameObject chunkObject;

    [NonSerialized] private MeshFilter meshFilter;
    [NonSerialized] private MeshCollider meshCollider;
    [NonSerialized] private MeshRenderer meshRenderer;
    [NonSerialized] private WorldGenerator world;

    [NonSerialized] private Vector3Int chunkPosition;

    [NonSerialized] private TerrainPoint[,,] terrainMap;

    //public TerrainPoint[,,] smallerTerrainMapForSer;

    public List<int>[,] savedTerrainPoints;

    [NonSerialized] private List<Vector3> vertices = new List<Vector3>();
    [NonSerialized] private List<int> triangles = new List<int>();
    [NonSerialized] private List<Color> colors = new List<Color>();
    [NonSerialized] private bool isLastXChunk = false;
    [NonSerialized] private bool isLastZChunk = false;
    [NonSerialized] private bool isFirstXChunk = false;
    [NonSerialized] private bool isFirstZChunk = false;

    [NonSerialized] private int width = GameData.ChunkWidth;
    [NonSerialized] private int height = GameData.ChunkHeight;
    [NonSerialized] public int curMaxHeight;
    [NonSerialized] private float terrainSurface = GameData.terrainSurface;

    public Chunk (Vector3Int _position, WorldGenerator n_world) {
        
        chunkObject = new GameObject();
        chunkObject.name = string.Format("Chunk {0}, {1}", _position.x, _position.z);
        chunkPosition = _position;
        world = n_world;
        chunkObject.transform.position = chunkPosition;

        if (chunkPosition.x == ((world.WorldSizeInChunks - 1) * GameData.ChunkWidth))
            isLastXChunk = true;
        if (chunkPosition.z == ((world.WorldSizeInChunks - 1) * GameData.ChunkWidth))
            isLastZChunk = true;
        if (chunkPosition.x == 0)
            isFirstXChunk = true;
        if (chunkPosition.z == 0)
            isFirstZChunk = true;


        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.material = world.worldMaterial;      //Resources.Load<Material>("Materials/Terrain");
        //meshRenderer.material.SetTexture("_TexArr", World.Instance.worldTexture);
        chunkObject.transform.tag = "Terrain";
        chunkObject.layer = 6;
        terrainMap = new TerrainPoint[width + 1, height + 1, width + 1];
        PopulateTerrainMap();
        CreateMeshData();

    }

    public void createFromData(Vector3Int _position, WorldGenerator n_world)
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        colors = new List<Color>();
        width = GameData.ChunkWidth;
        height = GameData.ChunkHeight;
        terrainSurface = GameData.terrainSurface;
        terrainMap = new TerrainPoint[width + 1, height + 1, width + 1];
        converFromSaveableToTerrainpoint(); //loads terrainpoints



        chunkObject = new GameObject();
        chunkObject.name = string.Format("Chunk {0}, {1}", _position.x, _position.z);
        chunkPosition = _position;
        world = n_world;
        chunkObject.transform.position = chunkPosition;

        if (chunkPosition.x == ((world.WorldSizeInChunks - 1) * GameData.ChunkWidth))
            isLastXChunk = true;
        if (chunkPosition.z == ((world.WorldSizeInChunks - 1) * GameData.ChunkWidth))
            isLastZChunk = true;
        if (chunkPosition.x == 0)
            isFirstXChunk = true;
        if (chunkPosition.z == 0)
            isFirstZChunk = true;


        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.material = world.worldMaterial;      //Resources.Load<Material>("Materials/Terrain");
        //meshRenderer.material.SetTexture("_TexArr", World.Instance.worldTexture);
        chunkObject.transform.tag = "Terrain";
        chunkObject.layer = 6;
        CreateMeshData();
    }

    public void CreateMeshData() {

        ClearMeshData();

        // Loop through each "cube" in our terrain.
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                for (int z = 0; z < width; z++) {

                    // Pass the value into our MarchCube function.
                    MarchCube(new Vector3Int(x, y, z));

                }
            }
        }

        BuildMesh();

    }

    void PopulateTerrainMap () {

        // The data points for terrain are stored at the corners of our "cubes", so the terrainMap needs to be 1 larger
        // than the width/height of our mesh.
        for (int x = 0; x < width + 1; x++) {
            for (int z = 0; z < width + 1; z++) {
                for (int y = 0; y < height + 1; y++) {

                    float thisHeight = GameData.GetTerrainHeight(x + chunkPosition.x, z + chunkPosition.z);
                    // Set the value of this point in the terrainMap.
                    float dist = y - thisHeight;
                    if (dist > terrainSurface)
                        dist = 1;
                    else
                        dist = 0;

                    if (y == 0)
                        dist = 1;
                    else if (isFirstXChunk && x == 0)
                        dist = 1;
                    else if (isFirstZChunk && z == 0)
                        dist = 1;
                    else if (isLastXChunk && x == width)
                        dist = 1;
                    else if (isLastZChunk && z == width)
                        dist = 1;

                    int colorval = 1;

                    if (thisHeight + 2 > curMaxHeight && dist == 0)
                        curMaxHeight = (int)thisHeight + 2;

                    terrainMap[x, y, z] = new TerrainPoint ((int)dist, colorval);

                }
            }
        }
    }

    void MarchCube (Vector3Int position) {

        // Sample terrain values at each corner of the cube.
        float[] cube = new float[8];
        for (int i = 0; i < 8; i++) {

            cube[i] = SampleTerrain(position + GameData.CornerTable[i]);

        }

        // Get the configuration index of this cube.
        int configIndex = GetCubeConfiguration(cube);

        // If the configuration of this cube is 0 or 255 (completely inside the terrain or completely outside of it) we don't need to do anything.
        if (configIndex == 0 || configIndex == 255)
            return;

        // Loop through the triangles. There are never more than 5 triangles to a cube and only three vertices to a triangle.
        //int edgeIndex = 0;

        int count = vertices.Count;
        //Debug.Log("poss| " + position.x +"|"+ position.y + "|" + position.z + " |col index:: " + terrainMap[position.x, position.y, position.z].textureID);
        Color curColor = world.theColorsOfTheWorld[terrainMap[position.x, position.y, position.z].textureID];
        int numbs = (GameData.TriangleTable[configIndex].Length) / 3;
        for (var i = 0; i < numbs; i++)
        {
            for (var j = 0; j < 3; j++)
            {
                int indice = GameData.TriangleTable[configIndex][i * 3 + j];
                Vector3 vert1 = position + GameData.CornerTable[GameData.EdgeIndexes[indice, 0]];
                Vector3 vert2 = position + GameData.CornerTable[GameData.EdgeIndexes[indice, 1]];
                vertices.Add((vert1 + vert2) / 2f);

            }
            colors.AddRange(new Color[] { curColor, curColor, curColor });
            triangles.AddRange(new int[] { i * 3 + count, i * 3 + count + 1, i * 3 + count +2});

        }
    }

    int GetCubeConfiguration (float[] cube) {

        // Starting with a configuration of zero, loop through each point in the cube and check if it is below the terrain surface.
        int configurationIndex = 0;
        for (int i = 0; i < 8; i++) {

            // If it is, use bit-magic to the set the corresponding bit to 1. So if only the 3rd point in the cube was below
            // the surface, the bit would look like 00100000, which represents the integer value 32.
            if (cube[i] == 1)
                configurationIndex |= 1 << i;

        }

        return configurationIndex;

    }

    public void changeTerrainMap(Vector3Int pos, int makeToGround, int newMaterial, GameData.modificationType modType)
    {
        //pos -= chunkPosition;
        if (pos.y < 0 || pos.y > GameData.ChunkHeight)
            return;

        if(makeToGround == 1)
        {
            terrainMap[pos.x, pos.y, pos.z].dstToSurface = 1;
            return;
        }

        int dist = makeToGround;
        switch (modType)
        {
            case GameData.modificationType.Both:
                if (pos.y == 0 || pos.y == GameData.ChunkHeight)
                    dist = 1;
                else if (isFirstXChunk && pos.x == 0)
                    dist = 1;
                else if (isFirstZChunk && pos.z == 0)
                    dist = 1;
                else if (isLastXChunk && pos.x == width)
                    dist = 1;
                else if (isLastZChunk && pos.z == width)
                    dist = 1;

                terrainMap[pos.x, pos.y, pos.z].dstToSurface = dist;
                if (dist != 1)
                {
                    //terrainMap[pos.x, pos.y, pos.z].textureID = newMaterial;
                    for (int i = 0; i < 8; i++)
                    {
                        Vector3Int newPos = pos - GameData.CornerTable[i];
                        if (newPos.x < 0 || newPos.z < 0)
                            continue;
                        terrainMap[newPos.x, newPos.y, newPos.z].textureID = newMaterial;
                    }
                }
                break;

            case GameData.modificationType.OnlyAir:
                if (terrainMap[pos.x, pos.y, pos.z].dstToSurface != 1)
                    return;

                if (pos.y == 0 || pos.y == GameData.ChunkHeight)
                    dist = 1;
                else if (isFirstXChunk && pos.x == 0)
                    dist = 1;
                else if (isFirstZChunk && pos.z == 0)
                    dist = 1;
                else if (isLastXChunk && pos.x == width)
                    dist = 1;
                else if (isLastZChunk && pos.z == width)
                    dist = 1;

                terrainMap[pos.x, pos.y, pos.z].dstToSurface = dist;
                if (dist != 1)
                {
                    //terrainMap[pos.x, pos.y, pos.z].textureID = newMaterial;
                    for (int i = 0; i < 8; i++)
                    {
                        Vector3Int newPos = pos - GameData.CornerTable[i];
                        if (newPos.x < 0 || newPos.z < 0)
                            continue;
                        terrainMap[newPos.x, newPos.y, newPos.z].textureID = newMaterial;
                    }
                }
                break;

            case GameData.modificationType.OnlyOtherBlocks:
                if (terrainMap[pos.x, pos.y, pos.z].dstToSurface != 0)
                    return;
                if (pos.y == 0 || pos.y == GameData.ChunkHeight)
                    dist = 1;
                else if (isFirstXChunk && pos.x == 0)
                    dist = 1;
                else if (isFirstZChunk && pos.z == 0)
                    dist = 1;
                else if (isLastXChunk && pos.x == width)
                    dist = 1;
                else if (isLastZChunk && pos.z == width)
                    dist = 1;

                terrainMap[pos.x, pos.y, pos.z].dstToSurface = dist;
                if (dist != 1)
                {
                    //terrainMap[pos.x, pos.y, pos.z].textureID = newMaterial;
                    for (int i = 0; i < 8; i++)
                    {
                        Vector3Int newPos = pos - GameData.CornerTable[i];
                        if (newPos.x < 0 || newPos.z < 0)
                            continue;
                        terrainMap[newPos.x, newPos.y, newPos.z].textureID = newMaterial;
                    }
                }
                break;
        }
            

        
    }

    public void changeTerrainMapInGame(Vector3Int pos)
    {
        
        terrainMap[pos.x, pos.y, pos.z].dstToSurface = 1;
    }

    public void convertTerrainMapIntoSerializealbe()
    {
        //smallerTerrainMapForSer = new TerrainPoint[width + 1, curMaxHeight, width + 1];
        savedTerrainPoints = new List<int>[width + 1, width + 1];

        //Debug.Log("terrainConvert:");
        //Debug.Log("whole length: " + smallerTerrainMapForSer.Length);
        //for (var i = 0; i < smallerTerrainMapForSer.Length; i++)
        //{
        //    Debug.Log("part '" + i + "' length: " + smallerTerrainMapForSer[i].Length);
        //}

        for (var x = 0; x < width + 1; x++)
        {
            for (var z = 0; z < width + 1; z++)
            {
                savedTerrainPoints[x, z] = new List<int>();

                int lastChangeColor = -1;

                for (var y = 0; y < height + 1; y++)
                {
                    int newSaveVal = terrainMap[x, y, z].dstToSurface == 1 ? (terrainMap[x, y, z].textureID+10) : terrainMap[x, y, z].textureID;
                    if (newSaveVal != lastChangeColor)
                    {
                        //Debug.Log("saving-- " + x.ToString() + " | " + y.ToString());
                        lastChangeColor = newSaveVal;
                        savedTerrainPoints[x, z].Add(lastChangeColor * (GameData.ChunkHeight + 1) + y);
                    }
                    //TerrainPoint curTerrPoist = terrainMap[x, y, z];
                    //smallerTerrainMapForSer[x, y, z] = new TerrainPoint(curTerrPoist.dstToSurface, curTerrPoist.textureID);

                }
            }
        }

        //for (var x = 0; x < width + 1; x++)
        //{
        //    for (var z = 0; z < width + 1; z++)
        //    {

        //        for (int i = 0; i < smallerTerrainMapForSer[x][z].Count; i++)
        //        {
        //            Debug.Log("SwapPoint for {" + x + "},{" + z + "}: " + smallerTerrainMapForSer[x][z][i]);
        //        }       
        //    }
        //}
    }
    private void converFromSaveableToTerrainpoint()
    {
        //curMaxHeight = height;
        for (var x = 0; x < width + 1; x++)
        {
            for (var z = 0; z < width + 1; z++)
            {
                //if(savedTerrainPoints[x, z].Count != 0)
                //{
                //    int firstToActuallyFillValue = savedTerrainPoints[x, z][0] % (GameData.ChunkHeight + 1);
                //    for (var y = 0; y < firstToActuallyFillValue; y++)
                //    {
                //        terrainMap[x, y, z] = new TerrainPoint(1, 0);
                //    }
                //}

                int lastFillVal = 0;
                for (var i = 0; i < savedTerrainPoints[x, z].Count; i++)
                {
                    if(i == savedTerrainPoints[x, z].Count - 1)
                    {
                        lastFillVal = savedTerrainPoints[x, z][i] % (GameData.ChunkHeight + 1);
                        break;
                    }

                    int currentColor = savedTerrainPoints[x, z][i] / (GameData.ChunkHeight + 1);
                    
                    int firstVal = savedTerrainPoints[x, z][i] % (GameData.ChunkHeight + 1);
                    int toFillVal = savedTerrainPoints[x, z][i + 1] % (GameData.ChunkHeight + 1);
                    for (var y = firstVal; y < toFillVal; y++)
                    {
                        if(currentColor >= 10)
                            terrainMap[x, y, z] = new TerrainPoint(1, (currentColor -10));
                        else
                            terrainMap[x, y, z] = new TerrainPoint(0, currentColor);
                    }
                }
                //if (lastFillVal > curMaxHeight)
                //    curMaxHeight = lastFillVal;
                for (var y = lastFillVal; y < height + 1; y++)
                {
                    terrainMap[x, y, z] = new TerrainPoint(1,1);
                }
                    //for (var y = 0; y < height + 1; y++)
                    //{

                    //    int currentChoose = 0;
                    //    int currentColor = 9;
                    //    if (y < savedTerrainPoints[x, z][currentChoose] % (GameData.ChunkHeight + 1))
                    //    {

                    //    }
                    //    else
                    //    {
                    //        currentColor = savedTerrainPoints[x, z][currentChoose] / (GameData.ChunkHeight + 1);
                    //    }

                    //    if (y >= curMaxHeight)
                    //    {
                    //        terrainMap[x, y, z] = new TerrainPoint(1, 0);
                    //    }
                    //    else
                    //    {
                    //        int currentChoose = -1;
                    //        int lastUsedColor = 9;

                    //        TerrainPoint curTerrPoist = smallerTerrainMapForSer[x, y, z];
                    //        terrainMap[x, y, z] = new TerrainPoint((int)curTerrPoist.dstToSurface, curTerrPoist.textureID);
                    //    }
                    //}
                }
        }
    }


    float SampleTerrain (Vector3Int point) {

        return terrainMap[point.x, point.y, point.z].dstToSurface;

    }

    void ClearMeshData () {

        vertices.Clear();
        triangles.Clear();
        colors.Clear();

    }

    void BuildMesh () {

        Mesh mesh = new Mesh();
        mesh.MarkDynamic();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        //mesh.triangles = mesh.triangles.Reverse().ToArray();
        //mesh.uv = uvs.ToArray();
        //mesh.RecalculateNormals();
        mesh.colors = colors.ToArray();

       
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.Optimize();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

    }

}
