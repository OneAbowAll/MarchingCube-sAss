using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;
using Debug = UnityEngine.Debug;

public class World : MonoBehaviour
{
    [Header("World Settings")]
    public float surfaceLevel = 0;
    public NoiseSettings noiseSettings;
    public Vector3Int worldScale = Vector3Int.zero;
    public Vector3Int chunkSize = Vector3Int.zero;

    [Header("Chunk Management")]
    public Camera playerCamera;
    public Transform player;
    public Chunk chunkPrefab;
    public int viewDistance;


    [Header("Editor & Debugging")]
    public bool autoRefresh;
    public bool debug;

    [HideInInspector]
    public bool noiseSettingsFoldout;
    [HideInInspector]
    public bool chunkSettingsFoldout;

    //Chunk management
    //List<Chunk> chunkHolder;
    Dictionary<Vector3Int, Chunk> chunkHolder;
    Queue<Chunk> reusableChunks;
    List<Chunk> existingChunks;


    Ray ScreenRay => Camera.main.ScreenPointToRay(Input.mousePosition);

    void Start()
    {
        //Initialize "buffers"
        chunkHolder = new Dictionary<Vector3Int, Chunk>();
        reusableChunks = new Queue<Chunk>();
        existingChunks = new List<Chunk>();

        GenerateChunk(Vector3Int.zero);
    }

    void Update()
    {
        //Check which chunk needs to be loaded or unloaded
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {

        Vector3Int playerInGrid = PointInGrid(player.position, chunkSize); playerInGrid.y = 0;

        //Remove unseen Chunks
        for (int i = 0; i < existingChunks.Count; i++)
        {
            float distance = Vector3Int.Distance(existingChunks[i].CurrentPosition, playerInGrid);
            Bounds chunkBound = CalculateBoundAt(existingChunks[i].CurrentPosition);
            if (!IsInView(chunkBound, playerCamera) && existingChunks[i].CurrentPosition != playerInGrid)
            {
                DeactivateChunk(existingChunks[i]);
            }
        }

        yield return null;

        //Load seen chunks
        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int z = -viewDistance; z <= viewDistance; z++)
            {
                Vector3Int chunkPosition = playerInGrid + (new Vector3Int(x, 0, z)*(chunkSize-Vector3Int.one));

                Bounds chunkBound = CalculateBoundAt(chunkPosition);

                if (chunkHolder.ContainsKey(chunkPosition))
                    continue;
                
                if (IsInView(chunkBound, playerCamera))
                {
                    if (reusableChunks.Count > 0)
                    {
                        Chunk reusableChunk = reusableChunks.Dequeue();
                        RelocateChunk(reusableChunk, chunkPosition);
                    }
                    else
                    {
                        GenerateChunk(chunkPosition);
                    }
                }
                yield return null;
            }
        }

    }

    //IEnumerator ChunkManagement()
    //{
    //    while (true)
    //    {
    //        Vector3Int playerInGrid = Vector3Int.zero; //PointInGrid(player.position, chunkSize); playerInGrid.y = 0;

    //        for (int x = -viewDistance; x <= viewDistance; x++)
    //        {
    //            for (int z = -viewDistance; z <= viewDistance; z++)
    //            {
    //                Vector3Int chunkPosition = new Vector3Int(playerInGrid.x + (x * (chunkSize.x - 1)),
    //                                                            0,
    //                                                            playerInGrid.z + (z * (chunkSize.z - 1)));

    //                Bounds chunkBound = CalculateBoundAt(chunkPosition);

    //                if (/*IsInView(chunkBound, Camera.main)*/ true)
    //                {

    //                    //Controlla se un chunk esiste in quella posizione e attivalo se lo trovi
    //                    Chunk possibileChunk = existingChunks.Find(chunk => chunk.CurrentPosition == chunkPosition);
    //                    if (possibileChunk != null)
    //                    {
    //                        if (possibileChunk.IsActive)
    //                            continue;
    //                        else
    //                        {
    //                            ActivateChunk(possibileChunk);
    //                            continue;
    //                        }
    //                    }
    //                    else
    //                    {
    //                        //In caso contrario prendine uno inutilizzato e spostaleo
    //                        possibileChunk = existingChunks.FindLast(chunk => !chunk.IsActive);
    //                        if (possibileChunk != null)
    //                        {

    //                            RelocateChunk(possibileChunk, chunkPosition);
    //                            continue;
    //                        }
    //                        else
    //                        {
    //                            //E se niente funziona generane uno da zero
    //                            GenerateChunk(chunkPosition);
    //                            continue;
    //                        }
    //                    }

    //                    yield return null;
    //                }
    //            }
    //        }

    //    }
    //}

    #region World Methods

    public void RefreshWorld()
    {
        if (Application.isPlaying)
        {
            ClearWorld();
        }
    }

    void ClearWorld()
    {
        for (int i = 0; i < existingChunks.Count; i++)
            Destroy(existingChunks[i].gameObject);

        chunkHolder.Clear();
        existingChunks.Clear();
    }

    #endregion

    #region Chunk Methods

    void GenerateChunk(Vector3Int position)
    {
        Chunk chunk = Instantiate(chunkPrefab, transform);

        chunk.Initialize(surfaceLevel, chunkSize, position, worldScale, noiseSettings);
        chunk.GenerateMesh();

        existingChunks.Add(chunk);
        chunkHolder.Add(position, chunk);
    }

    void RelocateChunk(Chunk chunk, Vector3Int to)
    {
        chunk.CurrentPosition = to;
        ActivateChunk(chunk);
    }

    void ActivateChunk(Chunk chunk)
    {
        chunk.IsActive = true;
        chunkHolder.Add(chunk.CurrentPosition, chunk);
        existingChunks.Add(chunk);
    }

    void DeactivateChunk(Chunk chunk)
    {
        chunk.IsActive = false;
        reusableChunks.Enqueue(chunk);
        chunkHolder.Remove(chunk.CurrentPosition);
        existingChunks.Remove(chunk);
    }

    #endregion

    #region Helpers

    bool IsInView(Bounds chunkBound, Camera camera)
    {
        Plane[] cameraPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(cameraPlanes, chunkBound);
    }

    Bounds CalculateBoundAt(Vector3Int position)
    {
        Vector3 size = chunkSize - Vector3.one;
        return new Bounds(position + (size / 2), size);
    }

    Vector3Int PointInGrid(Vector3 point, Vector3Int cellSize)
    {
        Vector3Int snap = new Vector3Int();
        snap.x = Mathf.FloorToInt(point.x / (cellSize.x - 1)) * (cellSize.x - 1);
        snap.y = Mathf.FloorToInt(point.y / (cellSize.y - 1)) * (cellSize.y - 1);
        snap.z = Mathf.FloorToInt(point.z / (cellSize.z - 1)) * (cellSize.z - 1);

        return snap;
    }

    #endregion

    #region Debug

    public int GetChunksOnScreen()
    {
        return existingChunks.Count;
    }

    public int GetChunksCreated()
    {
        return chunkHolder.Count;
    }

    void OnDrawGizmos()
    {
        
        if (debug)
        {
            Gizmos.color = Color.red;
            for (int x = 0; x < worldScale.x; x++)
            for (int y = 0; y < worldScale.y; y++)
            for (int z = 0; z < worldScale.z; z++)
                Gizmos.DrawWireCube(new Vector3(((x) * (chunkSize.x - 1)) + 7.5f,
                                                ((y) * (chunkSize.y - 1)),
                                                ((z) * (chunkSize.z - 1)) + 7.5f), new Vector3(15, 15, 15));
        }
    }

    #endregion

    /*
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


    private void OnDrawGizmos()
    {

        Gizmos.color = Color.red;
        for (int x = 0; x < worldScale.x; x++)
        {
            for (int y = 0; y < worldScale.y; y++)
            {
                for (int z = 0; z < worldScale.z; z++)
                {
                    Gizmos.DrawWireCube(new Vector3(((x) * (chunkSize.x-1))+7.5f, 
                                                    ((y) * (chunkSize.y-1)),
                                                    ((z) * (chunkSize.z-1))+ 7.5f), new Vector3(15, 15, 15));
                }
            }
        }
    }

    */
}