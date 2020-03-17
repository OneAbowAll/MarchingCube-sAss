using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    float surfaceLevel = 0;
    Vector3 chunkSize = Vector3.zero;
    Vector3Int currentPosition = Vector3Int.zero;
    Vector3Int worldScale = Vector3Int.zero;

    Mesh mesh;
    NoiseSettings noise;

    //float[] voxelValues;
    float[,,] voxelValues;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

    MeshFilter meshFilter;
    MeshCollider meshCollider;

    public Vector3Int CurrentPosition
    {
        get => currentPosition;
        set {
            currentPosition = value;
            transform.position = value;
        }
    }

    public int ChunkId
    {
        get;
        private set;
    }
    
    bool isActive;
    public bool IsActive
    {
        get => isActive;
        set{
            isActive = value;
            if (isActive)
                UpdateSharedMesh(mesh!=null?mesh:null);
            else
                UpdateSharedMesh(null);

        }
    }

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
    }

    public void Initialize(float surfaceLevel, Vector3Int chunkSize, Vector3Int currentPosition, Vector3Int worldScale, NoiseSettings noise)
    {
        this.surfaceLevel = surfaceLevel;
        this.chunkSize = chunkSize;
        CurrentPosition = currentPosition;
        this.worldScale = worldScale;
        this.noise = noise;

        IsActive = true;
        ChunkId = (int)(currentPosition.x + currentPosition.y * worldScale.x + currentPosition.z * worldScale.x * worldScale.y);

        mesh = new Mesh();

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        voxelValues = new float[chunkSize.x, chunkSize.y, chunkSize.z];
        GenerateValueGrid();
    }
    
    void GenerateValueGrid()
    {
        for (int x = 0; x < chunkSize.x; x++)
        {
            for (int y = 0; y < chunkSize.y; y++)
            {
                for (int z = 0; z < chunkSize.z; z++)
                {


                    //float nX = x / (chunkSize.x - 1f);
                    //float nY = y / (chunkSize.y - 1f);
                    //float nZ = z / (chunkSize.z - 1f);

                    float nX = (x + currentPosition.x) / (worldScale.x * (chunkSize.x - 1f)); //(x + (chunkSize.x * currentPosition.x)) / ((worldSize.x * chunkSize.x) - 1);
                    float nY = (y + currentPosition.y) / (worldScale.y * (chunkSize.y - 1f)); //(y + (chunkSize.y * currentPosition.y)) / ((worldSize.y * chunkSize.y) - 1);
                    float nZ = (z + currentPosition.z) / (worldScale.z * (chunkSize.z - 1f)); //(z + (chunkSize.z * currentPosition.z)) / ((worldSize.z * chunkSize.z) - 1);

                    //Debug.Log(nX);

                    //int index = (int)(x + y * chunkSize.x + z * chunkSize.x * chunkSize.y);

                    voxelValues[x, y, z] = y + noise.Generate(new Vector3(nX, nY, nZ)); //Perlin.Noise(new Vector3(nX, nY, nZ) * frequency) * amplitude;

                    //float nX = x / (chunkSize.x);
                    //float nY = y / (chunkSize.y);
                    //float nZ = z / (chunkSize.z);

                    //float density = y;
                    //density += Perlin.Noise(new Vector3(nX, nY, nZ) / frequency) * amplitude;
                    //density += Perlin.Noise(new Vector3(nX, nY, nZ) / frequency * 2f) * amplitude / 3f;

                    //voxelValues[x, y, z] = density;
                }
            }
        }
    }

    public void GenerateMesh()
    {
        mesh.Clear();
        triangles.Clear();
        vertices.Clear();


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

        for (int i = 0; i < vertices.Count; i++)
            triangles.Add(i);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        UpdateSharedMesh(mesh);
        //DebugChunk();
    }

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


    public void SetVoxel(Vector3Int position, float value)
    {
        voxelValues[position.x, position.y, position.z] = value;
    }

    public float GetVoxel(Vector3Int position)
    {
        return voxelValues[position.x, position.y, position.z];
    }


    void DebugChunk()
    {
        Debug.Log("Vertices in chunk: " + vertices.Count);
        Debug.Log("Voxels in chunk: " + voxelValues.Length);
        Debug.Log("Chunk size: " + chunkSize);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(245, 20, 0, 0.3f);
        //Gizmos.DrawSphere(test, 1.5f);
    }
}

/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    float surfaceLevel = 0;
    Vector3Int chunkSize = Vector3Int.zero;
    Vector3 currentPosition = Vector3.zero;
    Vector3Int worldSize = Vector3Int.zero;

    float frequency = 3f; //How quickly the values change
    float amplitude = 12f; //How strong is the noise

    Mesh mesh;

    //float[] voxelValues;
    float[,,] voxelValues;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

    MeshFilter meshFilter;
    MeshCollider meshCollider;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
    }

    public void Initialize(float surfaceLevel, Vector3Int chunkSize, Vector3 currentPosition, Vector3Int worldSize)
    {
        this.surfaceLevel = surfaceLevel;
        this.chunkSize = chunkSize;
        this.currentPosition = currentPosition;
        this.worldSize = worldSize;
        transform.position = currentPosition;

        mesh = new Mesh();

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        //voxelValues = new float[(int)(chunkSize.x * chunkSize.y * chunkSize.z)];
        voxelValues = new float[chunkSize.x, chunkSize.y, chunkSize.z];
        ValueGrid();
    }

    public Mesh GetMesh()
    {
        if(mesh.vertices.Length == 0)
            GenerateMesh();

        return mesh;
    }

    public float GetVoxel(int x, int y, int z)
    {
        //Debug.Assert(!((int)(x + y * chunkSize.x + z * chunkSize.x * chunkSize.y) >= voxelValues.Length),
        //            "IDIOT: " + (int)(x + y * chunkSize.x + z * chunkSize.x * chunkSize.y) + " " +
        //            "X: " + x + " | Y: " + y + " | Z: " + z);

        return voxelValues[x, y, z];
        //return voxelValues[(int)(x + y * chunkSize.x + z * chunkSize.x * chunkSize.y)];
    }

    public Vector3 GetPosition()
    {
        return currentPosition;
    }

    void ValueGrid()
    {
        for (int x = 0; x < chunkSize.x; x++)
        {
            for (int y = 0; y < chunkSize.y; y++)
            {
                for (int z = 0; z < chunkSize.z; z++)
                {
                    //float nX = x / (chunkSize.x - 1f);
                    //float nY = y / (chunkSize.y - 1f);
                    //float nZ = z / (chunkSize.z - 1f);

                    float nX = (x + (chunkSize.x*currentPosition.x)) / ((worldSize.x*chunkSize.x) - 1);
                    float nY = (y + (chunkSize.y*currentPosition.y)) / ((worldSize.y*chunkSize.y) - 1);
                    float nZ = (z + (chunkSize.z*currentPosition.z)) / ((worldSize.z*chunkSize.z) - 1);

                    //Debug.Log(nX);

                    //int index = (int)(x + y * chunkSize.x + z * chunkSize.x * chunkSize.y);

                    voxelValues[x, y, z] = y + Perlin.Noise(new Vector3(nX, nY, nZ) * 12) * 3; //y; //+ density;
                    //voxelValues[index] = Density.Calculate(nX, nY, nZ); //y; //+ density;
                    //Debug.Log(voxelValues[index]);
                }
            }
        }
    }

    public void GenerateMesh()
    {
        mesh.Clear();
        triangles.Clear();
        vertices.Clear();

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

                        if (voxelValues[ix, iy, iz] == World.Instance.GetVoxelValue(ix + (int)currentPosition.x, iy + (int)currentPosition.y, iz + (int)currentPosition.z))
                            Debug.Log("S");

                        cubeValues[i] = voxelValues[ix, iy, iz]; //World.Instance.GetVoxelValue(ix + (int)currentPosition.x, iy + (int)currentPosition.y, iz + (int)currentPosition.z);
                        //cubeValues[i] = voxelValues[(int)(ix + iy * chunkSize.x + iz * chunkSize.x * chunkSize.y)];  //World.Instance.GetVoxelValue(ix + (int)currentPosition.x, iy + (int)currentPosition.y, iz + (int)currentPosition.z); //voxelValues[(int)(ix + iy * chunkSize.x + iz * chunkSize.x * chunkSize.y)]; 
                        //Debug.Log("")
                        //Debug.Log("X: " + (ix + (int)currentPosition.x) + "| Y:" + (iy + (int)currentPosition.x) + "| Z:" + (iz + (int)currentPosition.z));
                    }

                    Cube cube = new Cube(cubeValues, surfaceLevel, new Vector3(x, y, z));
                    vertices.AddRange(cube.GenerateVertices(out int trianglesGenerted));
                }
            }
        }

        for (int i = 0; i < vertices.Count; i++)
            triangles.Add(i);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;

        //DebugChunk();
    }

    void DebugChunk()
    {
        Debug.Log("Vertices in chunk: " + vertices.Count);
        Debug.Log("Voxels in chunk: " + voxelValues.Length);
        Debug.Log("Chunk size: " + chunkSize);
    }
}
*/