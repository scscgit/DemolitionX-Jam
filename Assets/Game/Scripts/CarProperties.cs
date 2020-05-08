using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Car Property")]
public class CarProperties : ScriptableObject
{
    public string name;
    public Material carMaterial;
    public Material TyreMaterial;
    public Material rimMaterial;
}
