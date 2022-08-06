using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PathCreator))]
[RequireComponent (typeof(MeshFilter))]
[RequireComponent(typeof (MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class RoadCreator : MonoBehaviour
{
    [Range(0.05f, 1.5f)]
    public float spacing = 1;
    public float roadWidth = 1f;
    public bool autoUpdate;
    public float tiling = 1f;

    public void UpdateRoad()
    {
        Path path = GetComponent<PathCreator>().path;
        Vector2[] points = path.CalculateEvenlySpacedPoints(spacing);
        Mesh mesh = CreateRoadMesh(points, path.IsClosed);
        GetComponent<MeshFilter>().mesh = mesh;
        int textureRepeat = Mathf.RoundToInt(tiling * points.Length * spacing * 0.05f);
        GetComponent<MeshRenderer>().sharedMaterial.mainTextureScale = new Vector2(1, textureRepeat);
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    Mesh CreateRoadMesh(Vector2[] points, bool isClosed)
    {
        Vector3[] vertices = new Vector3[points.Length * 2];
        Vector2[] uvs = new Vector2[vertices.Length];
        int numTris = 2 * (points.Length - 1) + ((isClosed) ? 2 : 0);
        int[] triangles = new int[numTris * 3];
        int vertIndex = 0;
        int triIndex = 0;
        for(int i = 0; i < points.Length; i++)
        {
            Vector2 forward = Vector2.zero;
            if (i < points.Length - 1 || isClosed)
            {
                forward += points[(i + 1) % points.Length] - points[i];
            }
            if (i > 0 || isClosed)
            {
                forward += points[i] - points[(i - 1 + points.Length)%points.Length];
            }
            forward.Normalize();
            Vector2 left = new Vector2(-forward.y, forward.x);
            vertices[vertIndex] = points[i] + left * roadWidth * 0.5f;
            vertices[vertIndex+1] = points[i] - left * roadWidth * 0.5f;

            float completionPercent = i / (float)(points.Length - 1);
            float v = 1 - Mathf.Abs(2 * completionPercent - 1);
            uvs[vertIndex] = new Vector2(0, v);
            uvs[vertIndex+1] = new Vector2(1, v);
            
            if (i < points.Length - 1 || isClosed){
                triangles[triIndex] = vertIndex;
                triangles[triIndex + 1] = (vertIndex + 2) % vertices.Length;
                triangles[triIndex + 2] = vertIndex + 1;

                triangles[triIndex + 3] = vertIndex + 1;
                triangles[triIndex + 4] = (vertIndex + 2) % vertices.Length;
                triangles[triIndex + 5] = (vertIndex + 3) % vertices.Length;
            }
            
            vertIndex += 2;
            triIndex += 6;
        }
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        return mesh;
    }
}
