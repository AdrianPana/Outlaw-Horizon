//using UnityEngine;

//[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
//public class WaterManager : MonoBehaviour
//{
//    private MeshFilter meshFilter;

//    void Awake()
//    {
//        meshFilter = GetComponent<MeshFilter>();
//    }

//    void Update()
//    {
//        Vector3[] vertices = meshFilter.mesh.vertices;
//        for (int i = 0; i < vertices.Length; i++)
//        {
//            vertices[i].y = WaveManager.Instance.GetWaveHeight(transform.position.x + vertices[i].x, transform.position.z + vertices[i].z);
//        }

//        meshFilter.mesh.vertices = vertices;
//        meshFilter.mesh.RecalculateNormals();
//    }
//}
