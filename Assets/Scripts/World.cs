﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    [Header("World Settings")]
    public float surfaceLevel = 0;
    public Vector3Int worldScale = Vector3Int.zero;
    public Vector3Int chunkSize = Vector3Int.zero;
    public Chunk chunkPrefab;
    public NoiseSettings noiseSettings;
    public ChunkManager chunkManager;

    public Transform player;

    List<Chunk> chunkHolder;
    List<Chunk> existingChunks;
    //Dictionary<Vector3Int, Chunk> existingChunks;
    Chunk[] chunks;
    Ray ScreenRay => Camera.main.ScreenPointToRay(Input.mousePosition);

    [Space]
    //Editor
    public bool autoRefresh;
    [HideInInspector]
    public bool noiseSettingsFoldout;
    [HideInInspector]
    public bool chunkSettingsFoldout;

    void Start()
    {
        chunkHolder = new List<Chunk>();
        existingChunks = new List<Chunk>();

        Vector3Int playerInGrid = SnapToGrid(player.position, chunkSize); playerInGrid.y = 0;
        GenerateChunk(playerInGrid);
        //existingChunks = new Dictionary<Vector3Int, Chunk>();
    }

    void Update()
    {
        StartCoroutine(ClearWorldChunks());
        StartCoroutine(ManageChunks());
    }

    IEnumerator ManageChunks()
    {
        Vector3Int playerInGrid = SnapToGrid(player.position, chunkSize); playerInGrid.y = 0;

        for (int x = -chunkManager.viewDistance; x <= chunkManager.viewDistance; x++)
        {
            for (int z = -chunkManager.viewDistance; z <= chunkManager.viewDistance; z++)
            {
                Vector3Int chunkPosition = new Vector3Int(  playerInGrid.x - (x * (chunkSize.x - 1)),
                                                            0,
                                                            playerInGrid.z - (z * (chunkSize.z - 1)));
                Bounds chunkBound = new Bounds(chunkPosition + (chunkSize / 2), chunkSize - Vector3.one);

                if (IsInView(chunkBound, Camera.main))
                {
                    Chunk oldChunk = existingChunks.Find(chunk => chunk.CurrentPosition == chunkPosition);
                    if (oldChunk != null)
                    {
                        if (!oldChunk.IsActive)
                        {
                            oldChunk.IsActive = true;
                            chunkHolder.Add(oldChunk);
                        }
                    }
                    else
                    {
                        oldChunk = existingChunks.FindLast(chunk => !chunk.IsActive);
                        if (oldChunk != null)
                        {
                            oldChunk.CurrentPosition = chunkPosition;
                            oldChunk.IsActive = true;
                            oldChunk.RefreshChunk();

                            chunkHolder.Add(oldChunk);
                        }
                        else
                        {
                            GenerateChunk(chunkPosition);
                        }
                    }
                }
                yield return null;
            }
        }
    }

    void GenerateChunk(Vector3Int position)
    {
        Chunk chunk = Instantiate(chunkPrefab, transform);

        chunk.Initialize(surfaceLevel, chunkSize, position, worldScale, noiseSettings);
        chunk.GenerateMesh();

        existingChunks.Add(chunk);
        chunkHolder.Add(chunk);
    }

    #region World Management

    void Initialize()
    {
        //chunks = new Chunk[(int)(worldScale.x * worldScale.y * worldScale.z)];
        chunkHolder = new List<Chunk>();
    }

    void GenerateWorld()
    {
        Initialize();

        InitializeChunks();

        foreach (Chunk chunk in chunks)
        {
            chunk.GenerateMesh();
        }
    }

    #endregion

    #region Chunk Management

    void InitializeChunks()
    {
        for (int x = 0; x < worldScale.x; x++)
        {
            for (int y = 0; y < worldScale.y; y++)
            {
                for (int z = 0; z < worldScale.z; z++)
                {
                    Vector3Int currentPosition = new Vector3Int(x * (chunkSize.x - 1), y * (chunkSize.y - 1), z * (chunkSize.z - 1)); //new Vector3(x, y, z);
                    Chunk chunk = Instantiate(chunkPrefab, transform);

                    chunk.Initialize(surfaceLevel, chunkSize, currentPosition, worldScale, noiseSettings);

                    //chunk.transform.position = currentPosition;
                    //chunk.GetComponent<MeshFilter>().sharedMesh = chunk.GetMesh();
                    //chunk.GetComponent<MeshCollider>().sharedMesh = chunk.GetMesh();
                    chunks[(int)(x + y * worldScale.x + z * worldScale.x * worldScale.y)] = chunk;
                }
            }
        }
    }
    
    IEnumerator RequestChunk()
    {
        yield return null;
        //Vector3Int playerInGrid = SnapToGrid(player.position, chunkSize); playerInGrid.y = 0;

        //for (int x = -chunkManager.viewDistance; x <= chunkManager.viewDistance; x++)
        //{
        //    for (int z = -chunkManager.viewDistance; z <= chunkManager.viewDistance; z++)
        //    {
        //        //bool found = false;
        //        Vector3Int chunkPosition = new Vector3Int(  playerInGrid.x - (x * (chunkSize.x - 1)),
        //                                                    0,
        //                                                    playerInGrid.z - (z * (chunkSize.z - 1)));

        //        Bounds chunkBound = new Bounds(chunkPosition + (chunkSize / 2), chunkSize - Vector3.one);

        //        if (IsInView(chunkBound, Camera.main))
        //        {
        //            //Se esite nella posizione corrente riattivalo
        //            if (existingChunks.TryGetValue(chunkPosition, out Chunk chunk) && !chunk.isActive)
        //            {
        //                chunk.Activate();
        //                chunkHolder.Add(chunk);
        //                break;
        //            }
        //            else //Posso usare un altro chunk creato precedentemente?
        //            {
        //                foreach (Chunk c in existingChunks.Values)
        //                {
        //                    if (!c.isActive && c.GetPosition() != chunkPosition)
        //                    {
        //                        //inattivo in un altra posizione
        //                        c.SetPosition(chunkPosition);
        //                        c.Activate();
        //                        c.GenerateMesh();
        //                        chunkHolder.Add(chunk);
        //                        break;
        //                    }
        //                }

        //                //Se non è stato trovato niente creane uno nuovo
        //                NewChunkAt(chunkPosition);
        //                break;
        //            }
        //        }
        //        yield return null;

        //        ////Check if chunk is already in array
        //        //for (int i = 0; i < chunkHolder.Count; i++)
        //        //    if (chunkHolder[i].GetPosition() == chunkPosition)
        //        //        found = true;

        //        //if (!found)
        //        //{
        //        //    Bounds chunkBound = new Bounds(chunkPosition + (chunkSize / 2), chunkSize - Vector3.one);
        //        //    if (IsInView(chunkBound, Camera.main))
        //        //    {
        //        //        NewChunkAt(chunkPosition);
        //        //    }
        //        //}
        //    }
        //}
    }

    IEnumerator ClearWorldChunks()
    {
        Vector3Int playerInGrid = SnapToGrid(player.position, chunkSize); playerInGrid.y = 0;
        for (int i = 0; i < chunkHolder.Count; i++)
        {
            Vector3Int chunkPosition = chunkHolder[i].CurrentPosition;
            float distance = Vector3Int.Distance(chunkPosition, playerInGrid);

            Bounds chunkBound = new Bounds(chunkPosition + (chunkSize / 2), chunkSize - Vector3.one);
            if (!IsInView(chunkBound, Camera.main))
            {
                //Destroy(chunkHolder[i].gameObject);
                chunkHolder[i].IsActive = false;
                chunkHolder.RemoveAt(i);
                yield return null;
            }
        }

    }

    void NewChunkAt(Vector3Int position)
    {
        Chunk chunk = Instantiate(chunkPrefab, transform);

        chunk.Initialize(surfaceLevel, chunkSize, position, worldScale, noiseSettings);
        chunk.GenerateMesh();

        existingChunks.Add(chunk);
        chunkHolder.Add(chunk);
    }

    void ClearChunks()
    {
        foreach (Chunk chunk in chunks)
            Destroy(chunk.gameObject);
    }

    bool IsInView(Bounds chunkBound, Camera camera)
    {
        Plane[] cameraPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(cameraPlanes, chunkBound);
    }

    public float GetVoxelValue(Vector3Int position)
    {
        Chunk relativeChunk = ChunkThatContains(position);
        int cX = position.x - relativeChunk.CurrentPosition.x;
        int cY = position.y - relativeChunk.CurrentPosition.y;
        int cZ = position.z - relativeChunk.CurrentPosition.z;

        cX = (int)Mathf.Clamp(cX, 0, chunkSize.x - 1);
        cY = (int)Mathf.Clamp(cY, 0, chunkSize.y - 1);
        cZ = (int)Mathf.Clamp(cZ, 0, chunkSize.z - 1);

        //Debug.Log("Chunk relative to point.", relativeChunk.gameObject);
        return relativeChunk.GetVoxel(new Vector3Int(cX, cY, cZ));
    }

    Chunk ChunkThatContains(Vector3Int position)
    {
        int ix = (position.x) / (chunkSize.x);
        int iy = (position.y) / (chunkSize.y);
        int iz = (position.z) / (chunkSize.z);
        
        //Debug.Log("X: " + x + " | " + "Y: " + y + " | " + "Z: " + z);
        return chunks[ix + iy*worldScale.x + iz * worldScale.x* worldScale.y];
    }
   
    #endregion

    public void RefreshWorld()
    {
        if (Application.isPlaying)
        {
            ClearChunks();
            GenerateWorld();
        }
    }

    Vector3Int SnapToGrid(Vector3 point, Vector3Int cellSize)
    {
        Vector3Int snap = new Vector3Int();
        snap.x = Mathf.FloorToInt(point.x / (cellSize.x - 1)) * (cellSize.x - 1);
        snap.y = Mathf.FloorToInt(point.y / (cellSize.y - 1)) * (cellSize.y - 1);
        snap.z = Mathf.FloorToInt(point.z / (cellSize.z - 1)) * (cellSize.z - 1);

        return snap;
    }

    Vector3 PointInGrid(Vector3 point, Vector3 cellSize)
    {
        return new Vector3( Mathf.FloorToInt(point.x / (cellSize.x - 1)),
                            Mathf.FloorToInt(point.y / (cellSize.y - 1)),
                            Mathf.FloorToInt(point.z / (cellSize.z - 1)));
    }

    //Debug stuff
    public int GetChunksOnScreen()
    {
        return chunkHolder.Count;
    }

    public int GetChunksCreated()
    {
        return existingChunks.Count;
    }


    //private void OnDrawGizmos()
    //{

    //    Gizmos.color = Color.red;
    //    for (int x = 0; x < worldScale.x; x++)
    //    {
    //        for (int y = 0; y < worldScale.y; y++)
    //        {
    //            for (int z = 0; z < worldScale.z; z++)
    //            {
    //                Gizmos.DrawWireCube(new Vector3(((x) * (chunkSize.x-1))+7.5f, 
    //                                                ((y) * (chunkSize.y-1)),
    //                                                ((z) * (chunkSize.z-1))+ 7.5f), new Vector3(15, 15, 15));
    //            }
    //        }
    //    }
    //}
}

/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public float surfaceLevel = 0;
    public Vector3Int worldSize = Vector3Int.zero;
    public Vector3Int chunkSize = Vector3Int.zero;
    public GameObject chunkPrefab;

    GameObject[] chunks;

    // Start is called before the first frame update
    void Start()
    {
        chunks = new GameObject[(int)(worldSize.x * worldSize.y * worldSize.z)];

        RefreshWorld();
    }

    // Update is called once per frame
    void Update()
    {
    }

    [ContextMenu("Refresh World")]
    void RefreshWorld()
    {
        if (chunks.Length > 0)
            ClearChunks();

        chunks = new GameObject[(int)(worldSize.x * worldSize.y * worldSize.z)];

        for (int x = 0; x < worldSize.x; x++)
        {
            for (int y = 0; y < worldSize.y; y++)
            {
                for (int z = 0; z < worldSize.z; z++)
                {
                    Vector3 currentPosition = new Vector3(x, y, z);
                    Chunk newChunk = new Chunk(surfaceLevel, chunkSize, currentPosition, worldSize);

                    GameObject chunk = Instantiate(chunkPrefab, transform);
                    chunk.transform.position = new Vector3(x * (chunkSize.x - 1), y * (chunkSize.y - 1), z * (chunkSize.z - 1));
                    chunk.GetComponent<MeshFilter>().sharedMesh = newChunk.GetMesh();
                    chunk.GetComponent<MeshCollider>().sharedMesh = newChunk.GetMesh();
                }
            }
        }
    }

    void GetVoxel(int x, int y, int z)
    {

    }

    Chunk ChunkThatContains(int x, int y, int z)
    {
        int ix = x / worldSize.x;
        int iy = y / worldSize.y;
        int iz = z / worldSize.z;

        return chunks[ix + iy + iz];
    }

    void ClearChunks()
    {
        foreach (var chunk in chunks)
            Destroy(chunk);
    }
}
*/