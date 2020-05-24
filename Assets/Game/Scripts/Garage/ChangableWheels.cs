using UnityEngine;
using System.Collections;

/// <summary>
/// Changes wheels (visual only) at runtime. It holds changable wheels as prefab in an array.
/// </summary>
[System.Serializable, CreateAssetMenu(menuName = "Changable Wheels")]
public class ChangableWheels : ScriptableObject
{
    #region singleton

    private static ChangableWheels instance;

    public static ChangableWheels Instance
    {
        get
        {
            if (instance == null) instance = Resources.Load("ScriptableObjects/ChangableWheels") as ChangableWheels;
            return instance;
        }
    }

    #endregion

    [System.Serializable]
    public class ChangableWheel
    {
        public GameObject[] wheel;
    }

    public ChangableWheel[] wheels;
}
