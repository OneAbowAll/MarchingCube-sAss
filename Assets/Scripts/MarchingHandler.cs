using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MarchingHandler : MonoBehaviour
{
    public float surfaceLevel = 0;
    public Vector3 chunkSize = Vector3.zero;
    public Vector3 worldSize = Vector3.zero;
    public float frequency = 0.5f; //How quickly the values change
    public float amplitude = 2f; //How strong is the noise

    Mesh mesh;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    float[] voxelValues;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

    GameObject[] chunks;

    void Start()
    {
        chunks = new GameObject[(int)(worldSize.x * worldSize.y * worldSize.z)];
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponents<MeshCollider>()[0];

        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        ValueGrid();
        GenerateMesh();

    }

    void ValueGrid()
    {
        voxelValues = new float[(int)(chunkSize.x * chunkSize.z * chunkSize.y)];
        for (int x = 0; x < chunkSize.x; x++)
        {
            for (int y = 0; y < chunkSize.y; y++)
            {
                for (int z = 0; z < chunkSize.z; z++)
                {
                    float nX = x / (chunkSize.x - 1f);
                    float nY = y / (chunkSize.y - 1f);
                    float nZ = z / (chunkSize.z - 1f);

                    int index = (int)(x + y * chunkSize.x + z * chunkSize.x * chunkSize.y);

                    float density = Perlin.Noise(new Vector3(nX, nY, nZ) * frequency) * amplitude;
                    density += Perlin.Noise(new Vector3(nX, nY, nZ) * frequency*2) * amplitude/2;
                    density += Perlin.Noise(new Vector3(nX, nY, nZ) * frequency*3) * amplitude/3;
                    voxelValues[index] = y + density;
                    //Debug.Log(voxelValues[index]);
                }
            }
        }
    }

    void GenerateMesh()
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

                        cubeValues[i] = voxelValues[(int)(ix + iy * chunkSize.x + iz * chunkSize.x * chunkSize.y)];
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

        meshCollider.sharedMesh = mesh;
        meshFilter.sharedMesh = mesh;
    }


    [ContextMenu("Reload Mesh")]
    void ReloadMesh()
    {
        ValueGrid();
        GenerateMesh();
    }
}