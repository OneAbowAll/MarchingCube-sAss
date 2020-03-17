using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ChunkManager : ScriptableObject
{
    public Chunk chunkPrefab;
    public int viewDistance;
}
