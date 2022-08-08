using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AnimalSurvival.Terrain {
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshCollider))]
    public class Chunk : MonoBehaviour
    {
        // float[,,] density;
        [SerializeField]
        public ComputeShader shader;

        List<Vector3> vertices;
        List<int> triangles;

        Mesh mesh;

        float voxelSize;
        Vector3Int chunkOffset;
        int chunkSize;
        int chunkHeight;
        Vector3 noisePosition;
        float noiseScale;
        CoroutineQueue actions;

        public Vector3Int ChunkOffset
        {
            get { return chunkOffset; }
        }

        private void Awake()
        {
            actions = new CoroutineQueue(this);
            actions.StartLoop();
        }
        public void Generate(Vector3Int chunkOffset, float voxelSize, int chunkSize, int chunkHeight, Vector3 noisePosition, float noiseScale)
        {
            if (actions == null)
            {
                actions = new CoroutineQueue(this);
                actions.StartLoop();
            }
            this.chunkOffset = chunkOffset;
            this.chunkSize = chunkSize;
            this.voxelSize = voxelSize;
            this.chunkHeight = chunkHeight;
            this.noisePosition = noisePosition;
            this.noiseScale = noiseScale;
            CreateMeshFromNoise();
        }

        public void Load()
        {
            // Load Noise
            CreateMeshFromNoise();
        }

        public void UnLoad()
        {
        }

        float[] density;

        #region actions
        IEnumerator CreateMeshFromNoiseRoutine()
        {
            mesh = new Mesh();
            vertices = new List<Vector3>();
            triangles = new List<int>();

            vertices.Clear();
            triangles.Clear();

            /*shader.SetFloat("noiseScale", noiseScale);
            shader.SetFloat("chunkSize", chunkSize);
            shader.SetFloat("chunkHeight", chunkHeight);
            shader.SetVector("chunkOffset", new Vector3(chunkOffset.x, chunkOffset.y, chunkOffset.z));
            shader.SetVector("noisePosition", noisePosition);

            density = new float[chunkSize * chunkHeight * chunkSize];
            ComputeBuffer buffer = new ComputeBuffer(density.Length, sizeof(float));
            buffer.SetData(density);
            int kernelHandle = shader.FindKernel("CSMain");
            shader.SetBuffer(kernelHandle, "density", buffer);
            shader.Dispatch(kernelHandle, chunkSize, chunkHeight, chunkSize);
            buffer.GetData(density);*/

            for (int x = 0; x < chunkSize - 1; x++)
            {
                for (int z = 0; z < chunkSize - 1; z++)
                {
                    for (int y = 0; y < chunkHeight - 1; y++)
                    {
                        CreateTrisForCube(x, y, z);
                    }
                }
                yield return null;
            }
            //buffer.Release();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            GetComponent<MeshFilter>().mesh = mesh;
            GetComponent<MeshCollider>().sharedMesh = mesh;
            yield return CoroutineQueueResult.PASS;
            yield break;
        }

        #endregion
        void CreateMeshFromNoise()
        {
            actions.EnqueueAction(CreateMeshFromNoiseRoutine(), "Loading Mesh");
        }

        void CreateTrisForCube(int x, int y, int z)
        {
            Vector3Int cubePos = new Vector3Int(x, y, z);
            int caseByte = CellCase(cubePos);
            int numTris = MarchingCubes.caseToNumPolys[caseByte];
            for (int tri = 0; tri < numTris; tri++)
            {
                Vector3Int edgesOfTri = MarchingCubes.edgeConnectList[caseByte, tri];
                Vector3 triVertexOnEdge1 = VertexFromInterpolatedNoise(edgesOfTri.x, cubePos);
                Vector3 triVertexOnEdge2 = VertexFromInterpolatedNoise(edgesOfTri.y, cubePos);
                Vector3 triVertexOnEdge3 = VertexFromInterpolatedNoise(edgesOfTri.z, cubePos);
                vertices.Add(triVertexOnEdge3);
                vertices.Add(triVertexOnEdge2);
                vertices.Add(triVertexOnEdge1);

                triangles.Add(vertices.Count - 3);
                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 1);

            }
        }

        Vector3 VertexFromInterpolatedNoise(int edgeId, Vector3Int cubePos)
        {
            int[] edgeCorners = MarchingCubes.EdgeToCorners(edgeId);
            Vector3Int cornerA = CubeCornersToChunkNoiseGridPoints(edgeCorners[0], cubePos);
            Vector3Int cornerB = CubeCornersToChunkNoiseGridPoints(edgeCorners[1], cubePos);
            float time = InterpolateTriangleVertexOnCubeEdge(cornerA, cornerB);
            Vector3 cornerAWorld = NoiseGridToWorldPos(cornerA);
            Vector3 cornerBWorld = NoiseGridToWorldPos(cornerB);
            return Vector3.Lerp(cornerAWorld, cornerBWorld, time);
        }

        Vector3 NoiseGridToWorldPos(Vector3Int point)
        {
            return new Vector3((float)point.x, (float)point.y, (float)point.z) * voxelSize;
        }

        float InterpolateTriangleVertexOnCubeEdge(Vector3Int cornerA, Vector3Int cornerB)
        {
            return Mathf.InverseLerp(TerrainGenerator.Instance.GetDensityAtLocalGridPoint(cornerA, this), TerrainGenerator.Instance.GetDensityAtLocalGridPoint(cornerB, this), 0f);
            //return Mathf.InverseLerp(density[to1D(cornerA)], density[to1D(cornerB)], 0f);
        }

        int to1D(Vector3Int point)
        {
            return (chunkHeight * chunkSize * point.z) + (point.y * chunkSize) + point.x;
        }

        Vector3Int CubeCornersToChunkNoiseGridPoints(int corner, Vector3Int cubePos)
        {
            return MarchingCubes.CornerToCubeVertex(corner) + cubePos;
        }

        int CellCase(Vector3Int v0)
        {
            // https://developer.nvidia.com/gpugems/gpugems3/part-i-geometry/chapter-1-generating-complex-procedural-terrains-using-gpu
            int v0x = v0.x, v0y = v0.y, v0z = v0.z;
            Vector3Int v1 = new Vector3Int(v0x, v0y + 1, v0z);
            Vector3Int v2 = new Vector3Int(v0x + 1, v0y + 1, v0z);
            Vector3Int v3 = new Vector3Int(v0x + 1, v0y, v0z);
            Vector3Int v4 = new Vector3Int(v0x, v0y, v0z + 1);
            Vector3Int v5 = new Vector3Int(v0x, v0y + 1, v0z + 1);
            Vector3Int v6 = new Vector3Int(v0x + 1, v0y + 1, v0z + 1);
            Vector3Int v7 = new Vector3Int(v0x + 1, v0y, v0z + 1);
            int caseByte = GetBit(v7) << 7 | GetBit(v6) << 6 |
                GetBit(v5) << 5 | GetBit(v4) << 4 |
                GetBit(v3) << 3 | GetBit(v2) << 2 |
                GetBit(v1) << 1 | GetBit(v0);
            return caseByte;
        }

        int GetBit(Vector3Int v)
        {
            return TerrainGenerator.Instance.GetDensityAtLocalGridPoint(v, this) < 0 ? 0 : 1;
            //return density[to1D(v)] < 0 ? 0 : 1;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(transform.position + TerrainGenerator.Instance.ChunkSizeInWorldSpace / 2f, TerrainGenerator.Instance.ChunkSizeInWorldSpace);
            Handles.Label(transform.position, chunkOffset + "");
            /*for(int x = 0; x < chunkSize; x++)
            {
                for(int y = 0; y < chunkHeight; y++)
                {
                    for(int z = 0; z < chunkSize; z++)
                    {
                        Vector3Int v = new Vector3Int(x, y, z);
                        if (TerrainGenerator.Instance.IsLocalGridPointOnChunkEdge(v) && z == 0)
                        {
                            Handles.color = Color.blue;
                            Vector3 offset = Vector3.down;
                            Vector3Int noiseoffset = Vector3Int.zero;
                            if (x == 0)
                            {
                                offset = Vector3.up;
                                noiseoffset = Vector3Int.left;
                            }
                            
                            Handles.Label(transform.position + NoiseGridToWorldPos(v) + offset,
                                TerrainGenerator.Instance.GetDensityAtLocalGridPoint(v, this));
                            Handles.Label(transform.position + NoiseGridToWorldPos(v) + offset * 0.5f, v + " -> " + TerrainGenerator.Instance.LocalToWorldGridPos(v, this));
                            
                        }
                    }
                }
            }*/
        }

        private void OnApplicationQuit()
        {
            Destroy(gameObject);
        }
    }
}