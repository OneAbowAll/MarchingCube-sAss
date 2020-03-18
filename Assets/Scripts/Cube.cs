using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube
{

    Vector3 positionRelativeToV0;
    float[] values = new float[8];

    float surfaceLevel = 0;
    int cubeIndex = 0;

    public Cube(float[] values, float surfaceLevel, Vector3 positionRelativeToV0, int cubeIndex = 0)
    {
        this.values = values;
        this.surfaceLevel = surfaceLevel;
        this.positionRelativeToV0 = positionRelativeToV0;

        if (cubeIndex == 0)
            this.cubeIndex = CalculateIndex();
        else
            this.cubeIndex = cubeIndex;
    }

    public Vector3[] GenerateVertices()
    {
        List<Vector3> vertices = new List<Vector3>();
        int[] edges = Table.triangles[cubeIndex];
        
        for (int i = 0; i < edges.Length; i++)
        {
            int indexA = Table.cornerFromEdge[edges[i]][0];
            int indexB = Table.cornerFromEdge[edges[i]][1];


            Vector3 edgePosition = interpolateVerts(corners[indexA], values[indexA], corners[indexB], values[indexB]); //(corners[indexA] + corners[indexB]) / 2;
            vertices.Add(edgePosition + positionRelativeToV0);
        }
        
        return vertices.ToArray();
    }

    int CalculateIndex()
    {
        int cubeIndex = 0;
        for (int i = 0; i < 8; i++)
        {
            if (values[i] < surfaceLevel)
                cubeIndex |= 1 << i;
        }
        return cubeIndex;
    }

    Vector3 interpolateVerts(Vector3 v1Pos, float v1Value, Vector3 v2Pos, float v2Value)
    {
        float t = (surfaceLevel - v1Value) / (v2Value - v1Value);
        return v1Pos + (t * (v2Pos - v1Pos));
    }

    public static Vector3[] corners = new Vector3[]
    {
        new Vector3(0, 0, 0), //0
        new Vector3(0, 1, 0), //1
        new Vector3(1, 1, 0), //2
        new Vector3(1, 0, 0), //3
        new Vector3(0, 0, 1), //4
        new Vector3(0, 1, 1), //5
        new Vector3(1, 1, 1), //6
        new Vector3(1, 0, 1)  //7
    };
}
