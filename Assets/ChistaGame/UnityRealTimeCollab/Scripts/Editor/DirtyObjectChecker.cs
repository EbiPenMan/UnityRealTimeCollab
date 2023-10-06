using UnityEditor;
using UnityEngine;

namespace ChistaGame.RealTimeCollab.Editor
{
    public class DirtyObjectChecker: EditorWindow
    {
        [MenuItem("Tools/Dirty Object Checker")]
        public static void ShowWindow()
        {
            GetWindow<DirtyObjectChecker>("Dirty Object Checker");
        }

        private void OnEnable()
        {
            // Register the event handler
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.projectChanged += OnHierarchyChanged;
        }

        private void OnDisable()
        {
            // Unregister the event handler to avoid memory leaks
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.projectChanged -= OnHierarchyChanged;
        }

        private void OnHierarchyChanged()
        {
            // Check for dirty objects and prevent changes if necessary
            Debug.unityLogger.Log($"DirtyObjectChecker | OnHierarchyChanged | start");
            CheckDirtyObjects();
        }

        private void CheckDirtyObjects()
        {
            GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject go in allGameObjects)
            {
                // You can implement your logic here to identify and check dirty objects.
                // For example, check object names, tags, or custom components.

                if (ShouldNotChange(go))
                {
                    // Prevent changes to the object or display a warning message.
                    Debug.LogWarning($"Object {go.name} should not be modified.");
                }
            }
            
            
            // Check if specific assets have been modified.
            string[] modifiedAssets = AssetDatabase.FindAssets("t:Prefab"); // Replace with your asset types and criteria.

            foreach (string guid in modifiedAssets)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                Debug.LogWarning($"Asset '{assetPath}' has been modified. Please do not change this asset.");
            }
            
        }

        // Replace this with your logic to determine if an object should not change.
        private bool ShouldNotChange(GameObject gameObject)
        {
            // For example, you can use a naming convention to identify objects that should not change.
            return gameObject.name.StartsWith("LOCKED_");
        }
    }
}