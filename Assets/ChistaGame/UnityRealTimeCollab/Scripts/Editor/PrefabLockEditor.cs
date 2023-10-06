using UnityEditor;
using UnityEngine;

namespace ChistaGame.RealTimeCollab.Editor
{
    // [CustomEditor(typeof(GameObject))]
    public class PrefabLockEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (Selection.activeGameObject != null && PrefabUtility.IsPartOfAnyPrefab(Selection.activeGameObject))
            {
                string prefabName = Selection.activeGameObject.name;
                if (prefabName.StartsWith("LOCKED_"))
                {
                    EditorGUILayout.HelpBox("This prefab is locked and should not be modified.", MessageType.Warning);
                }
            }

            DrawDefaultInspector();
        }
    }
}