using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct MarchingJob : IJob
{
    public NativeArray<float> VoxelValues;
    public NativeArray<Vector3> Vertices;
    public Vector3 ChunkSize;
    public float SurfaceLevel;

    public void Execute()
    {
        for (int x = 0; x < ChunkSize.x - 1; x++)
        {
            for (int y = 0; y < ChunkSize.y - 1; y++)
            {
                for (int z = 0; z < ChunkSize.z - 1; z++)
                {
                    float[] cubeValues = new float[8];
                    for (int i = 0; i < 8; i++)
                    {

                        int ix = x + (int)Cube.corners[i].x;
                        int iy = y + (int)Cube.corners[i].y;
                        int iz = z + (int)Cube.corners[i].z;

                        cubeValues[i] = VoxelValues[x + y*(int)ChunkSize.x + z* (int)ChunkSize.y* (int)ChunkSize.x];
                    }

                    Cube cube = new Cube(cubeValues, SurfaceLevel, new Vector3(x, y, z));
                    cube.GenerateVertices(out Vertices);
                }
            }
        }
    }
}
