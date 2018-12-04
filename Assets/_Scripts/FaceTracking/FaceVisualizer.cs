using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class FaceVisualizer : MonoBehaviour {

	[SerializeField]
	private MeshFilter meshFilter;
	
	private Mesh faceDebugMesh;
	private UnityARSessionNativeInterface m_session;

	Bounds bounds;
	Vector3 basePos;
	GameObject bBox;

	public Material faceBoxMaterial;


	// Use this for initialization
	void Start () {


		m_session = UnityARSessionNativeInterface.GetARSessionNativeInterface();

		Application.targetFrameRate = 60;
		ARKitFaceTrackingConfiguration config = new ARKitFaceTrackingConfiguration();
		config.alignment = UnityARAlignment.UnityARAlignmentGravity;
		config.enableLightEstimation = true;

        //Debug.Log("arkit face session configured");

		if (config.IsSupported) {
			
			m_session.RunWithConfig (config);

			UnityARSessionNativeInterface.ARFaceAnchorAddedEvent += FaceAdded;
			UnityARSessionNativeInterface.ARFaceAnchorUpdatedEvent += FaceUpdated;
			UnityARSessionNativeInterface.ARFaceAnchorRemovedEvent += FaceRemoved;

		}
		

	}

	void FaceAdded (ARFaceAnchor anchorData)
	{
		gameObject.transform.localPosition = UnityARMatrixOps.GetPosition (anchorData.transform);
		gameObject.transform.localRotation = UnityARMatrixOps.GetRotation (anchorData.transform);
        //Debug.Log("face added");
	}

	void FaceUpdated (ARFaceAnchor anchorData)
	{

		gameObject.transform.localPosition = UnityARMatrixOps.GetPosition (anchorData.transform);
		gameObject.transform.localRotation = UnityARMatrixOps.GetRotation (anchorData.transform);

		//draw face mesh
		updateDebugFaceMesh(anchorData);
        //Debug.Log("face updated");
	}


	void FaceRemoved (ARFaceAnchor anchorData)
	{
		meshFilter.mesh = null;
		faceDebugMesh = null;
        //Debug.Log("face removed");
	}	


	void createDebugFaceMesh(ARFaceAnchor anchorData){
		
        //Debug.Log("create debug face mesh");
        faceDebugMesh = new Mesh ();
		meshFilter.mesh = faceDebugMesh;

		//drawBBox();
		updateDebugFaceMesh(anchorData);

	}


	void updateDebugFaceMesh(ARFaceAnchor anchorData){

        //Debug.Log("update debug face mesh");

		if(faceDebugMesh == null){
			createDebugFaceMesh(anchorData);
		}
		faceDebugMesh.vertices = anchorData.faceGeometry.vertices;
		faceDebugMesh.uv = anchorData.faceGeometry.textureCoordinates;
		faceDebugMesh.triangles = anchorData.faceGeometry.triangleIndices;
		faceDebugMesh.RecalculateBounds();
		faceDebugMesh.RecalculateNormals();
			
		// updateBBox(faceDebugMesh);
		// bBox.transform.position = bounds.center;


	}

	void drawBBox(){
		bounds = this.GetComponent<MeshRenderer>().bounds;
		basePos = bounds.center;
		bBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
		bBox.transform.parent = transform;
		bBox.transform.position = basePos;
		bBox.transform.localScale = new Vector3(0.3f, 0.25f, 0.11f);
		// bBox.transform.rotation = new Quaternion(-10f,10f,-86f, 0f);
		// bBox.transform.position = new Vector3(0,0.02f,0);
		bBox.GetComponent<Renderer>().material = faceBoxMaterial;
	}

}
