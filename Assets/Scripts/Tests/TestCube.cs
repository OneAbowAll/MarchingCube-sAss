using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCube : MonoBehaviour
{
    [Range(0, 255)]
    public int cubeIndex = 0;
    public Vector3 offset;

    MeshFilter meshFilter;
    Mesh mesh;

    public List<int> triangles = new List<int>();
    public List<Vector3> vertices = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
    }

    // Update is called once per frame
    void Update()
    {
        mesh.Clear();
        triangles.Clear();
        vertices.Clear();

        if (cubeIndex == 0)
            return;

        Cube cube = new Cube(new float[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0, offset, cubeIndex);
        vertices.AddRange(cube.GenerateVertices());

        //Add triangles
        int lastT = 0;
        for (int i = 0; i < vertices.Count; i++)
        {
            int t = lastT + i;
            triangles.Add(t);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    private void OnDrawGizmos()
    {
        //So che non è efficente, ma fa il suo sporco lavoro, in più è solo per fare testing

        int c0 = cubeIndex & 1;
        int c1 = cubeIndex & 2;
        int c2 = cubeIndex & 4;
        int c3 = cubeIndex & 8;
        int c4 = cubeIndex & 16;
        int c5 = cubeIndex & 32;
        int c6 = cubeIndex & 64;
        int c7 = cubeIndex & 128;

        Gizmos.color = Color.red;

        //0
        if(c0 != 0)
            Gizmos.DrawSphere(Cube.corners[0], 0.1f);

        //1
        if (c1 != 0)
            Gizmos.DrawSphere(Cube.corners[1], 0.1f);

        //2
        if (c2 != 0)
            Gizmos.DrawSphere(Cube.corners[2], 0.1f);

        //3
        if (c3 != 0)
            Gizmos.DrawSphere(Cube.corners[3], 0.1f);

        //4
        if (c4 != 0)
            Gizmos.DrawSphere(Cube.corners[4], 0.1f);

        //5
        if (c5 != 0)
            Gizmos.DrawSphere(Cube.corners[5], 0.1f);

        //6
        if (c6 != 0)
            Gizmos.DrawSphere(Cube.corners[6], 0.1f);
        
        //7
        if (c6 != 0)
            Gizmos.DrawSphere(Cube.corners[7], 0.1f);
    }

}
