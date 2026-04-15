using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour {

	public float springForce = 20f;
	public float damping = 5f;

	Mesh deformingMesh;
	Vector3[] originalVertices, displacedVertices;
	Vector3[] vertexVelocities;

	float uniformScale = 1f;

	void Start () {
		deformingMesh = GetComponent<MeshFilter>().mesh;
		originalVertices = deformingMesh.vertices;
		displacedVertices = new Vector3[originalVertices.Length];
		for (int i = 0; i < originalVertices.Length; i++) {
			displacedVertices[i] = originalVertices[i];
		}
		vertexVelocities = new Vector3[originalVertices.Length];
	}

	void Update () {
		uniformScale = transform.localScale.x;
		for (int i = 0; i < displacedVertices.Length; i++) {
			UpdateVertex(i);
		}
		deformingMesh.vertices = displacedVertices;
		deformingMesh.RecalculateNormals();
	}

	void UpdateVertex (int i) {
		Vector3 velocity = vertexVelocities[i];
		Vector3 displacement = displacedVertices[i] - originalVertices[i];
		displacement *= uniformScale;
		velocity -= displacement * springForce * Time.deltaTime;
		velocity *= 1f - damping * Time.deltaTime;
		vertexVelocities[i] = velocity;
		displacedVertices[i] += velocity * (Time.deltaTime / uniformScale);
	}

	public void AddDeformingForce (Vector3 point, float force) {
		point = transform.InverseTransformPoint(point);
		for (int i = 0; i < displacedVertices.Length; i++) {
			AddForceToVertex(i, point, force);
		}
	}

	void AddForceToVertex (int i, Vector3 point, float force) {
		Vector3 pointToVertex = displacedVertices[i] - point;
		pointToVertex *= uniformScale;
		float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
		float velocity = attenuatedForce * Time.deltaTime;
		vertexVelocities[i] += pointToVertex.normalized * velocity;
	}

	/// <summary>
	/// Applies a one-time permanent inward deformation at worldPoint.
	/// The deformation moves vertices toward the hit point and then updates the rest pose
	/// so the mesh stays deformed (no spring back).
	/// </summary>
	/// <param name="worldPoint">World-space hit point</param>
	/// <param name="force">Base force controlling deformation magnitude</param>
	/// <param name="inwardScale">Multiplier for inward displacement intensity (0..1)</param>
	public void ApplyPermanentDeformation(Vector3 worldPoint, float force, float inwardScale = 0.1f) {
		if (deformingMesh == null || displacedVertices == null || originalVertices == null) return;

		Vector3 localPoint = transform.InverseTransformPoint(worldPoint);

		for (int i = 0; i < displacedVertices.Length; i++) {
			Vector3 pointToVertex = displacedVertices[i] - localPoint;
			pointToVertex *= uniformScale;

			float sqrDist = pointToVertex.sqrMagnitude;
			if (sqrDist == 0f) sqrDist = 0.0001f;

			// Move vertices inward (towards the hit point) to create a small indentation
			Vector3 inwardDir = -pointToVertex.normalized;
			float attenuated = force / (1f + sqrDist);
			float displacementAmount = attenuated * inwardScale;

			displacedVertices[i] += inwardDir * displacementAmount;

			// Zero velocity for stability and set new rest pose so deformation stays
			vertexVelocities[i] = Vector3.zero;
			originalVertices[i] = displacedVertices[i];
		}

		// Immediately update mesh and normals so change is visible at once
		deformingMesh.vertices = displacedVertices;
		deformingMesh.RecalculateNormals();
	}
}