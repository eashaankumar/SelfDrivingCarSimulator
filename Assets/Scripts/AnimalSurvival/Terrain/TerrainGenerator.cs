using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnimalSurvival.Terrain
{
    public class TerrainGenerator : MonoBehaviour
    {
        [SerializeField, Range(0, 20)]
        int chunkSize;
        [SerializeField, Range(0, 20)]
        int chunkHeight;

        [SerializeField]
        GameObject voxelPrefab;
        [SerializeField]
        float voxelSize;
        [SerializeField]
        float noiseScale;
        [SerializeField]
        Vector3 noisePosition;

        [SerializeField]
        Chunk prefab;

        [SerializeField, Tooltip("Chunk load/unloads for this target")]
        Transform target;

        [SerializeField]
        float loadedTerrainDistance;

        [SerializeField]
        float loadedTerrainDistanceUp;

        [SerializeField]
        float loadedTerrainDistanceDown;

        Vector3 chunkSizeInWorldSpace;

        Dictionary<Vector3Int, Chunk> chunkMap;

        public static TerrainGenerator Instance;

        Dictionary<Vector3Int, float> noise;

        public Vector3 ChunkSizeInWorldSpace
        {
            get { return chunkSizeInWorldSpace; }
        }

        // Start is called before the first frame update
        void Start()
        {
            //StartCoroutine(Generate());
            Instance = this;
            MarchingCubes.Init();
            noise = new Dictionary<Vector3Int, float>();
            chunkSizeInWorldSpace = new Vector3((chunkSize - 1) * voxelSize, (chunkHeight - 1) * voxelSize, (chunkSize - 1) * voxelSize);
            chunkMap = new Dictionary<Vector3Int, Chunk>();

            StartCoroutine(LoadChunks());
            StartCoroutine(UnLoadChunks());

        }

        IEnumerator LoadChunks()
        {
            while (true)
            {
                for (float distance = 0; distance < loadedTerrainDistance; distance += 1f)
                {
                    for (float angle = 0; angle < 360f; angle += 1f)
                    {
                        for (float upDistance = 0; upDistance < loadedTerrainDistanceUp; upDistance += 1f)
                        {
                            for (float upAngle = 0; upAngle < 180f; upAngle += 1f)
                            {
                                Vector3 targetPos = target.transform.position + Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * distance + Quaternion.AngleAxis(upAngle, Vector3.right) * Vector3.forward * upDistance;
                                SpawnChunkForWorldPos(targetPos);
                            }
                        }
                        for (float downDistance = 0; downDistance < loadedTerrainDistanceUp; downDistance += 2f)
                        {
                            for (float downAngle = 0; downAngle < 180f; downAngle += 1f)
                            {
                                Vector3 targetPos = target.transform.position + Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * distance + Quaternion.AngleAxis(-downAngle, Vector3.right) * Vector3.forward * downDistance;
                                SpawnChunkForWorldPos(targetPos);
                            }
                        }
                        yield return null;
                    }
                }
                yield return null;
            }
        }

        IEnumerator UnLoadChunks()
        {
            while (true)
            {
                List<Vector3Int> outOfViewChunks = new List<Vector3Int>();
                foreach (Chunk c in chunkMap.Values)
                {
                    if (!IsChunkInViewDistance(c, target.transform.position))
                    {
                        outOfViewChunks.Add(c.ChunkOffset);
                    }
                }
                foreach (Vector3Int chunkId in outOfViewChunks)
                {
                    Chunk c;
                    chunkMap.Remove(chunkId, out c);
                    if (c == null)
                    {
                        throw new System.Exception("Chunk " + chunkId + " could not be unloaded because it isn't currently loaded.");
                    }
                    else
                    {
                        c.UnLoad();
                        Destroy(c.gameObject);
                    }
                }
                yield return null;
            }
        }

        bool IsChunkInViewDistance(Chunk c, Vector3 viewer)
        {
            Vector3 pointInChunk = c.transform.position;
            for(float x = 0; x < chunkSizeInWorldSpace.x; x += voxelSize)
            {
                for(float y = 0; y < chunkSizeInWorldSpace.y; y += voxelSize)
                {
                    for (float z = 0; z < chunkSizeInWorldSpace.z; z += voxelSize)
                    {
                        pointInChunk = c.transform.position + new Vector3(x, y, z);
                        Vector3 direction = pointInChunk - viewer;
                        float sqrDistance = (direction).sqrMagnitude;
                        direction.Normalize();
                        Vector3 pointOnViewEllipsoid = new Vector3(direction.x * loadedTerrainDistance,
                                                                    direction.y * (direction.y > 0 ? loadedTerrainDistanceUp : loadedTerrainDistanceDown),
                                                                    direction.z * loadedTerrainDistance);
                        float possibleDistanceSqr = pointOnViewEllipsoid.sqrMagnitude;
                        if (sqrDistance < possibleDistanceSqr)
                        {
                            return true;
                        }
                    } 
                }
            }

            return false;
        }

        void SpawnChunkForWorldPos(Vector3 worldPos)
        {
            Vector3Int chunkPos = GetChunkFromWorldPos(worldPos);
            if (chunkMap.ContainsKey(chunkPos)) return;
            Chunk c = Instantiate(prefab, ChunkPosInWorld(worldPos), Quaternion.identity);
            c.Generate(chunkPos, voxelSize, chunkSize, chunkHeight, noisePosition, noiseScale);
            chunkMap.Add(chunkPos, c);
        }

        Vector3 ChunkPosInWorld(Vector3 worldPos)
        {
            Vector3 ngo = GetChunkFromWorldPos(worldPos);
            return new Vector3((ngo.x * (chunkSizeInWorldSpace.x)),
                                (ngo.y * (chunkSizeInWorldSpace.y)),
                                (ngo.z * (chunkSizeInWorldSpace.z)));
        }

        Vector3Int GetChunkFromWorldPos(Vector3 worldPos)
        {
            return new Vector3Int(Mathf.FloorToInt(worldPos.x / (chunkSizeInWorldSpace.x)),
                                Mathf.FloorToInt(worldPos.y / (chunkSizeInWorldSpace.y)),
                                Mathf.FloorToInt(worldPos.z / (chunkSizeInWorldSpace.z)));
        }

        Vector3 ChunkPositionLocalGridToWorld(Vector3Int localGridPos, Chunk c)
        {
            Vector3 gridPos = new Vector3(localGridPos.x, localGridPos.y, localGridPos.z) * voxelSize;
            return c.transform.position + gridPos;
        }

        public Vector3Int LocalToWorldGridPos(Vector3Int localGridPos, Chunk callingChunk)
        {
            return new Vector3Int(localGridPos.x + callingChunk.ChunkOffset.x * (chunkSize-1),
                                             localGridPos.y + callingChunk.ChunkOffset.y * (chunkHeight-1),
                                             localGridPos.z + callingChunk.ChunkOffset.z * (chunkSize-1));
        }

        public float GetDensityAtLocalGridPoint(Vector3Int localGridPos, Chunk callingChunk)
        {
            Vector3Int worldGridPos = LocalToWorldGridPos(localGridPos, callingChunk);
            if (!noise.ContainsKey(worldGridPos))
            {
                Vector3 noiseSample = new Vector3(worldGridPos.x, worldGridPos.y, worldGridPos.z) + noisePosition;
                //float n = Noise.PerlinNoise.Get3DPerlinNoise(noiseSample, noiseScale);
                float n = SphericalNoise(noiseSample, noiseScale);
                noise.Add(worldGridPos, n);
            }
            return noise.GetValueOrDefault(worldGridPos);
        }

        float SphericalNoise(Vector3 noiseSample, float noiseScale)
        {
            float radius = 1f * noiseScale;
            return Mathf.Lerp(-1f, 1f, Mathf.InverseLerp(0, radius, noiseSample.magnitude)) + Random.Range(-1f, 1f) * 0.05f; // InvLerp: 0 if at center or planet, 1 if at surface of planet
        }

        public bool IsLocalGridPointOnChunkEdge(Vector3Int gridPos)
        {
            return (float)gridPos.x % (chunkSize-1) == 0f || 
                (float)gridPos.y % (chunkHeight-1) == 0f || 
                (float)gridPos.z % (chunkSize-1) == 0f;
        }

        
        // Update is called once per frame
        void Update()
        {
        }

        private void OnDrawGizmos()
        {
            /*foreach (Chunk c in chunkMap.Values) {
                for (int x = 0; x < chunkSize; x++)
                {
                    for (int z = 0; z < chunkSize; z++)
                    {
                        for (int y = 0; y < chunkHeight; y++)
                        {
                            Vector3Int grid = new Vector3Int(x, y, z);
                            if (IsLocalGridPointOnChunkEdge(grid))
                            {
                                float density = GetDensityAtLocalGridPoint(grid, c);
                                print(density);
                                Gizmos.color = Color.Lerp(Color.white, Color.red, (density + 1) / 2f);
                                Gizmos.DrawSphere(ChunkPositionLocalGridToWorld(grid, c), 0.01f);
                            }

                        }

                    }
                }
            }*/
        }
    }
}