using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parabox.CSG;
using EzySlice;

/// <summary>
/// Tool for cutting a hole from a base mesh using a subtractive mesh.
/// Returns both the removed piece and the remaining base mesh.
/// </summary>
public class ProBuilderBooleanTool : MonoBehaviour
{
    [Header("Meshes")]
    public GameObject baseMesh;
    [SerializeField] private GameObject subtractiveMesh;

    [Header("Settings")]
    [SerializeField] private Transform fragmentsParent;
    [SerializeField] private LayerMask defaultLayer;

    #region Public Methods

    /// <summary>
    /// Subtracts the subtractiveMesh from baseMesh and returns both the remaining mesh and removed part.
    /// </summary>
    public (GameObject remainingMesh, GameObject removedPart) CutHoleWithResult(GameObject mesh, GameObject subMesh)
    {
        baseMesh = mesh;
        subtractiveMesh = subMesh;
        if (baseMesh == null || subtractiveMesh == null)
        {
            Debug.LogError("Base mesh or subtractive mesh not assigned.");
            return (null, null);
        }

        // Copy baseMesh to avoid modifying original
        GameObject baseCopy = Instantiate(baseMesh, baseMesh.transform.position, baseMesh.transform.rotation);
        baseCopy.transform.localScale = baseMesh.transform.lossyScale;
        baseCopy.name = baseMesh.name + "_WithHole";

        // --- 1) Removed piece (intersection) ---
        GameObject removedPart = new GameObject(baseMesh.name + "_RemovedPart");
        removedPart.transform.SetParent(fragmentsParent);
        removedPart.transform.position = Vector3.zero;
        removedPart.transform.rotation = Quaternion.identity;
        removedPart.layer = defaultLayer;

        Model intersectModel = CSG.Intersect(baseCopy, subtractiveMesh);
        if (intersectModel != null && intersectModel.mesh != null)
        {
            removedPart.AddComponent<MeshFilter>().sharedMesh = intersectModel.mesh;
            removedPart.AddComponent<MeshRenderer>().sharedMaterials = intersectModel.materials.ToArray();
        }
        else
        {
            Destroy(removedPart);
            removedPart = null;
        }

        // --- 2) Remaining base mesh (subtract hole) ---
        Model subtractedModel = CSG.Subtract(baseCopy, subtractiveMesh);

        GameObject remainingMesh = new GameObject(baseMesh.name + "_Remaining");
        remainingMesh.transform.SetParent(fragmentsParent);
        remainingMesh.transform.position = Vector3.zero;
        remainingMesh.transform.rotation = Quaternion.identity;
        remainingMesh.layer = defaultLayer;

        remainingMesh.AddComponent<MeshFilter>().sharedMesh = subtractedModel.mesh;
        remainingMesh.AddComponent<MeshRenderer>().sharedMaterials = subtractedModel.materials.ToArray();

        // Hide original base mesh
        baseMesh.SetActive(false);

        Destroy(baseCopy);

        return (remainingMesh, removedPart);
    }

    #endregion
}