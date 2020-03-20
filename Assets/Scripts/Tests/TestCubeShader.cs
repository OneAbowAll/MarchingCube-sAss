using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCubeShader : MonoBehaviour
{
    [Range(0, 255)]
    public int cubeIndex = 0;
    public ComputeShader shader;

    int kernel;
    int numMaxTri;

    MeshFilter meshFilter;
    Mesh mesh;

    //[SerializeField] List<int> triangles = new List<int>();
    //[SerializeField] List<Vector3> vertices = new List<Vector3>();

    void Awake()
    {
        mesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
    }

    void Start()
    {
        kernel = shader.FindKernel("MarchCube");
    }

    // Update is called once per frame
    void Update()
    {
        GetVertices();
    }

    void GetVertices()
    {
        mesh.Clear();

        ComputeBuffer triBuffer = new ComputeBuffer(5, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        ComputeBuffer triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

        triBuffer.SetCounterValue(0);
        shader.SetBuffer(kernel, "triangles", triBuffer);
        shader.SetInt("cubeIndex", cubeIndex);
        shader.Dispatch(0, 1, 1, 1);
        
        ComputeBuffer.CopyCount(triBuffer, triCountBuffer, 0);

        int[] triCountArray = { 0 };
        triCountBuffer.GetData(triCountArray);
        int numTris = triCountArray[0];

        Triangle[] tris = new Triangle[numTris];
        triBuffer.GetData(tris, 0, 0, numTris);

        var vertices = new Vector3[numTris * 3];
        var triangles = new int[numTris * 3];

        for (int i = 0; i < numTris; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                triangles[i * 3 + j] = i * 3 + j;
                vertices[i * 3 + j] = tris[i][j];
            }
        }

        triBuffer.Release();
        triCountBuffer.Release();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
}

public struct Triangle
{
    public Vector3 a;
    public Vector3 b;
    public Vector3 c;

    public Vector3 this[int i] {
        get
        {
            switch (i)
            {
                case 0: return a;
                case 1: return b;
                default: return c;
            }
        }
    }

}

