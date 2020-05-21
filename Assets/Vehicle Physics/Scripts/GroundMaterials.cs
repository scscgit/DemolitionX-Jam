using UnityEngine;
using System.Collections;

///<summary>
///Class that contains necessary details for variable wheel frictions!
///This also has variables that handle the audio at different surfaces! 
///</summary>
[System.Serializable]
public class GroundMaterials : ScriptableObject
{
    public static GroundMaterials instance;

    public static GroundMaterials Instance
    {
        get
        {
            if (instance == null) instance = Resources.Load("ScriptableObjects/GroundMaterials") as GroundMaterials;
            return instance;
        }
    }

    [System.Serializable]
    public class GroundMaterialFrictions
    {
        public PhysicMaterial groundMaterial;
        public float forwardStiffness = 1f;
        public float sidewaysStiffness = 1f;
        public float slip = .25f;
        public float damp = 1f;
        public GameObject groundParticles;
        public AudioClip groundSound;
    }

    public GroundMaterialFrictions[] frictions;

    public bool useTerrainSplatMapForGroundFrictions = false;
    public PhysicMaterial terrainPhysicMaterial;
    public int[] terrainSplatMapIndex;
}
