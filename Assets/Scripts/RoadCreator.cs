using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


[RequireComponent(typeof(PathCreator))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RoadCreator : MonoBehaviour
{
    [Range(0.05f, 2.0f)]
    public float spacing = 1;
    public float roadWidth = 1;
    public bool autoUpdate;
    public float tiling = 1;


    public void UpdateRoad() {
        Path path = GetComponent<PathCreator>().path;
        Vector2[] points = path.CalculateEvenSpacedPoint(spacing);
        GetComponent<MeshFilter>().mesh = CreateRoadMesh(points, path.IsClosed);

        int textureRepeat = Mathf.RoundToInt(tiling * points.Length * spacing * 0.05f);
        GetComponent<MeshRenderer>().sharedMaterial.mainTextureScale = new Vector2(1, textureRepeat);
    }

    Mesh CreateRoadMesh(Vector2[] points, bool isclosed) {

        Vector3[] verts = new Vector3[points.Length*2];
        Vector2[] uvs = new Vector2[verts.Length];
        int triNum = 2 * (points.Length - 1) + (isclosed ? 2 : 0);
        int[] tris = new int[triNum*3];
        int vertIndx = 0;
        int triIndx = 0;

        for (int i = 0; i < points.Length; i++) {

            Vector2 forward = Vector2.zero;


            if (i < points.Length - 1 || isclosed)  forward += points[(i+1) % points.Length] - points[i];
            if (i > 0 || isclosed) forward += points[i] - points[(i-1 + points.Length) % points.Length];

            forward.Normalize();
            Vector2 left = new Vector2(-forward.y, forward.x);

            verts[vertIndx] = points[i] + left * roadWidth * 0.5f;
            verts[vertIndx + 1] = points[i] - left * roadWidth * 0.5f;

            float compPercent = i / (float)(points.Length - 1);
            float v = 1 - Mathf.Abs(2*compPercent-1);
            uvs[vertIndx] = new Vector2(0, v);
            uvs[vertIndx+1] = new Vector2(1, v);

            if (i < points.Length - 1 || isclosed) {
                tris[triIndx] = vertIndx;
                tris[triIndx+1] = (vertIndx+2) % verts.Length;
                tris[triIndx+2] = vertIndx+1;

                tris[triIndx + 3] = vertIndx + 1;
                tris[triIndx + 4] = (vertIndx + 2) % verts.Length;
                tris[triIndx + 5] = (vertIndx + 3) % verts.Length;
            }

            vertIndx += 2;
            triIndx += 6;

        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
        return mesh;

    }

}
