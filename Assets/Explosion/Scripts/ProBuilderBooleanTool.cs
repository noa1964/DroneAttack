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

        // Create an unparented world-space copy of the subtractive mesh to ensure correct transforms
        GameObject subCopy = Instantiate(subtractiveMesh, subtractiveMesh.transform.position, subtractiveMesh.transform.rotation);
        subCopy.transform.localScale = subtractiveMesh.transform.lossyScale;
        subCopy.transform.SetParent(null);
        subCopy.name = subtractiveMesh.name + "_CSGCopy";

        // --- 1) Removed piece (intersection) ---
        GameObject removedPart = new GameObject(baseMesh.name + "_RemovedPart");
        removedPart.transform.SetParent(fragmentsParent);
        removedPart.transform.position = Vector3.zero;
        removedPart.transform.rotation = Quaternion.identity;
        removedPart.layer = Mathf.RoundToInt(Mathf.Log(defaultLayer.value, 2));

        Model intersectModel = CSG.Intersect(baseCopy, subCopy);
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
        Model subtractedModel = CSG.Subtract(baseCopy, subCopy);

        GameObject remainingMesh = null;
        if (subtractedModel != null && subtractedModel.mesh != null)
        {
            remainingMesh = new GameObject(baseMesh.name + "_Remaining");
            remainingMesh.transform.SetParent(fragmentsParent);
            remainingMesh.transform.position = Vector3.zero;
            remainingMesh.transform.rotation = Quaternion.identity;
            remainingMesh.layer = Mathf.RoundToInt(Mathf.Log(defaultLayer.value, 2));

            remainingMesh.AddComponent<MeshFilter>().sharedMesh = subtractedModel.mesh;
            remainingMesh.AddComponent<MeshRenderer>().sharedMaterials = subtractedModel.materials.ToArray();
        }
        else
        {
            Debug.LogWarning($"CSG.Subtract returned null or empty mesh for '{baseMesh.name}' with subtractor '{subtractiveMesh.name}'.")
;
        }

        // Hide original base mesh
        baseMesh.SetActive(false);

        // Clean up temporary copies
        Destroy(baseCopy);
        Destroy(subCopy);

        // Sanity check: sometimes CSG can produce unexpectedly large 'removed' piece.
        // Ensure the removed part is the smaller fragment; if it's larger than the remaining piece, swap.
        if (remainingMesh != null && removedPart != null)
        {
            float remVol = ComputeMeshVolumeSafe(remainingMesh.GetComponent<MeshFilter>()?.sharedMesh, remainingMesh.transform.lossyScale);
            float remPartVol = ComputeMeshVolumeSafe(removedPart.GetComponent<MeshFilter>()?.sharedMesh, removedPart.transform.lossyScale);


            if (remPartVol > remVol * 1.1f) // if removed is significantly larger than remaining, swap them
            {
                Debug.LogWarning("[ProBuilderBooleanTool] Detected removed part larger than remaining part — swapping tuple to preserve semantics.");
                var tmp = remainingMesh;
                remainingMesh = removedPart;
                removedPart = tmp;
            }
        }

        return (remainingMesh, removedPart);
    }

    private float ComputeMeshVolumeSafe(Mesh mesh, Vector3 lossyScale)
    {
        if (mesh == null) return 0f;
        Vector3 size = mesh.bounds.size;
        size = new Vector3(Mathf.Abs(size.x * lossyScale.x), Mathf.Abs(size.y * lossyScale.y), Mathf.Abs(size.z * lossyScale.z));
        return size.x * size.y * size.z;
    }

    public void CutHole()
    {

        if (baseMesh == null || subtractiveMesh == null)
        {
            Debug.LogError("Base mesh or subtractive mesh not assigned.");
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
        removedPart.layer = Mathf.RoundToInt(Mathf.Log(defaultLayer.value, 2));

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
        remainingMesh.layer = Mathf.RoundToInt(Mathf.Log(defaultLayer.value, 2));

        remainingMesh.AddComponent<MeshFilter>().sharedMesh = subtractedModel.mesh;
        remainingMesh.AddComponent<MeshRenderer>().sharedMaterials = subtractedModel.materials.ToArray();

        // Hide original base mesh
        baseMesh.SetActive(false);

        Destroy(baseCopy);
    }


}