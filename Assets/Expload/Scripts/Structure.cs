using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour {

    [SerializeField] private Transform _origin;
    /// <summary>
    /// Splitting the mesh of the structure into triangles calling 
    /// the static function Triangulate of MeshTriangulator and passing 
    /// the structure transform
    /// </summary>
    void OnMouseDown(){
        MeshTriangulator.Explode(transform, _origin.position);
    }
}
