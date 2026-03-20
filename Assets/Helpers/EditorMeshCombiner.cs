using UnityEngine;
using UnityEditor;

public class EditorMeshCombiner
{
    [MenuItem("Tools/Combine Selected Meshes")]
    static void CombineMeshes()
    {
        GameObject parent = Selection.activeGameObject;
        if (parent == null) return;

        MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>();

        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        Material mat = null;

        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i].transform == parent.transform) continue;

            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = parent.transform.worldToLocalMatrix *
                                   meshFilters[i].transform.localToWorldMatrix;

            if (mat == null)
                mat = meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial;
        }

        Mesh mesh = new Mesh();
        mesh.name = parent.name + "_Combined";
        mesh.CombineMeshes(combine);

        // Save mesh as asset
        AssetDatabase.CreateAsset(mesh, "Assets/" + mesh.name + ".asset");

        // Assign to parent
        MeshFilter mf = parent.GetComponent<MeshFilter>();
        if (!mf) mf = parent.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;

        MeshRenderer mr = parent.GetComponent<MeshRenderer>();
        if (!mr) mr = parent.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;

        Debug.Log("Meshes combined!");
    }
}
