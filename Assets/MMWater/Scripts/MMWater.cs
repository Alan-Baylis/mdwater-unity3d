﻿using UnityEngine;
using System.Collections;

public class MMWater : MonoBehaviour {

    void Awake()
    {
        InitializeWaterMeshes();
    }

	void Start ()
    {
	}
	
	void Update ()
    {
	}

    private void InitializeWaterMeshes()
    {
        MeshFilter meshFilter = transform.GetComponent<MeshFilter>();
        meshFilter.mesh.Clear();
        Mesh newMesh = MMMeshCreator.CreateMesh(2);
        meshFilter.mesh = newMesh;
    }

    [ContextMenu("Test")]
    void Test()
    {
    }
}
