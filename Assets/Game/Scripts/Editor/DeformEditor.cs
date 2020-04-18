using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.Scripts;
using System;

[CustomEditor(typeof(DeformCarMesh))]
public class DeformEditor : Editor
{
    DeformCarMesh d;

    public override void OnInspectorGUI()
    {
        d = (DeformCarMesh)target;

        DrawDefaultInspector();

        if(GUILayout.Button("Make Colliders"))
        {
            Array.Resize(ref d.colliders, d.meshFilters.Length);
            for(int i = 0; i <= d.meshFilters.Length - 1; i++)
            {
                if(!d.meshFilters[i].GetComponent<MeshCollider>()) 
                {
                    MeshCollider col = d.meshFilters[i].gameObject.AddComponent<MeshCollider>();
                    col.convex = true;
                }
                
                d.colliders[i] = d.meshFilters[i].gameObject.GetComponent<MeshCollider>();
            }            
        }
    }
}
