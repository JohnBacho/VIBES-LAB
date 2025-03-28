using UnityEngine;

public class Pyramid : MonoBehaviour {
    public float height = 1;
    public float width = 1;
    public float length = 1;

    void Start() {
        var meshFilter = GetComponent<MeshFilter>();
        var mesh = new Mesh();

        var widthOffset = width * 0.5f;
        var lengthOffset = length * 0.5f;

        var points = new Vector3[] {
            new Vector3(-widthOffset, 0, -lengthOffset),
            new Vector3(widthOffset, 0, -lengthOffset),
            new Vector3(widthOffset, 0, lengthOffset),
            new Vector3(-widthOffset, 0, lengthOffset),
            new Vector3(0, height, 0)
        };

        mesh.vertices = new Vector3[] {
            points[0], points[1], points[2],
            points[0], points[2], points[3],
            points[0], points[1], points[4],
            points[1], points[2], points[4],
            points[2], points[3], points[4],
            points[3], points[0], points[4]
        };

        mesh.triangles = new int[] {
            0, 1, 2,
            3, 4, 5,
            8, 7, 6,
            11, 10, 9,
            14, 13, 12,
            17, 16, 15
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();

        meshFilter.mesh = mesh;
    }
}