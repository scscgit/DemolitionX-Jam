using UnityEditor;
using UnityEngine;

namespace Game.Scripts.Editor
{
    /// <summary>
    /// Courtesy of https://forum.unity.com/threads/hotkey-to-reset-transform-on-game-object.601684/
    /// </summary>
    public static class ResetGameObjectPosition
    {
        [MenuItem("GameObject/Reset Transform #r")]
        public static void MoveSceneViewCamera()
        {
            GameObject[] selectedObjects = Selection.gameObjects;

            foreach (var selectedObject in selectedObjects)
            {
                Undo.RegisterCompleteObjectUndo(selectedObject.transform, "Reset game object to origin");

                var pPos = Vector3.zero;
                var pRot = Quaternion.identity;
                var pScale = Vector3.one;

                if (selectedObject.transform.parent != null)
                {
                    var parent = selectedObject.transform.parent;
                    pPos = parent.position;
                    pRot = parent.rotation;
                    pScale = parent.localScale;
                }

                selectedObject.transform.position = Vector3.zero + pPos;
                selectedObject.transform.rotation = Quaternion.identity * pRot;
                selectedObject.transform.localScale = Vector3.one;
            }
        }
    }
}
