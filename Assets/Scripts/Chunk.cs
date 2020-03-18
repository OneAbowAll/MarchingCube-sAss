using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    float surfaceLevel = 0;
    Vector3 chunkSize = Vector3.zero;
    Vector3Int currentPosition = Vector3Int.zero;
    Vector3Int worldScale = Vector3Int.zero;
    NoiseSettings noise;

    //Generation values
    float[,,] voxelValues;

    //Mesh data
    Mesh mesh;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

    MeshFilter meshFilter;
    MeshCollider meshCollider;

    //Chunk management
    bool isActive;

    #region Setters / Getters

    public Vector3Int CurrentPosition
    {
        get => currentPosition;
        set {
            transform.position = currentPosition = value;
        }
    }
    
    public bool IsActive
    {
        get => isActive;
        set{
            isActive = value;
            if (isActive)
                GenerateMesh();
            else
                UpdateSharedMesh(null);

        }
    }

    #endregion

    void Awake()
    {
        //Initialize chunk
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
    }

    public void Initialize(float surfaceLevel, Vector3Int chunkSize, Vector3Int currentPosition, Vector3Int worldScale, NoiseSettings noise)
    {
        this.surfaceLevel = surfaceLevel;
        this.chunkSize = chunkSize;
        this.worldScale = worldScale;
        this.noise = noise;


        CurrentPosition = currentPosition;
        IsActive = true;

        mesh = new Mesh();
        voxelValues = new float[chunkSize.x, chunkSize.y, chunkSize.z];

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GenerateValueGrid();
    }

    #region Chunk Generation

    void GenerateValueGrid()
    {
        for (int x = 0; x < chunkSize.x; x++)
        {
            for (int y = 0; y < chunkSize.y; y++)
            {
                for (int z = 0; z < chunkSize.z; z++)
                {
                    float nX = (x + currentPosition.x) / (worldScale.x * (chunkSize.x - 1f));
                    float nY = (y + currentPosition.y) / (worldScale.y * (chunkSize.y - 1f));
                    float nZ = (z + currentPosition.z) / (worldScale.z * (chunkSize.z - 1f));

                    voxelValues[x, y, z] = y + noise.Generate(new Vector3(nX, nY, nZ));
                }
            }
        }
    }

    public void GenerateMesh()
    {
        mesh.Clear();
        triangles.Clear();
        vertices.Clear();

        //March
        for (int x = 0; x < chunkSize.x - 1; x++)
        {
            for (int y = 0; y < chunkSize.y - 1; y++)
            {
                for (int z = 0; z < chunkSize.z - 1; z++)
                {
                    float[] cubeValues = new float[8];
                    for (int i = 0; i < 8; i++)
                    {

                        int ix = x + (int)Cube.corners[i].x;
                        int iy = y + (int)Cube.corners[i].y;
                        int iz = z + (int)Cube.corners[i].z;

                        cubeValues[i] = voxelValues[ix, iy, iz];
                    }

                    Cube cube = new Cube(cubeValues, surfaceLevel, new Vector3(x, y, z));
                    vertices.AddRange(cube.GenerateVertices());
                }
            }
        }

        //Add triangles
        for (int i = 0; i < vertices.Count; i++)
            triangles.Add(i);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        UpdateSharedMesh(mesh);
    }

    #endregion

    #region Chunk Management

    public void RefreshChunk()
    {
        GenerateValueGrid();
        GenerateMesh();
    }

    void UpdateSharedMesh(Mesh mesh)
    {
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    #endregion

    #region Debug

    void DebugChunk()
    {
        Debug.Log("Vertices in chunk: " + vertices.Count);
        Debug.Log("Voxels in chunk: " + voxelValues.Length);
        Debug.Log("Chunk size: " + chunkSize);
    }

    #endregion 
}