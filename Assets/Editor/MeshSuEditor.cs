using UnityEditor;
using UnityEngine;
using MeshSuFracturer;
namespace MeshSuFracturerTool {
    public class MeshFractureWindow : EditorWindow
    {
        private GameObject selectedObject;
        private MeshSu meshSuComponent;

        private int cutCascades = 1;
        private bool combineAllFractures = true;
        private bool destroyMeshRightNow = false;
        private bool destroyOnCollision = false;
        private float neededCollisionForceToDestroy = 10f;
        private float explodeForce = 100f;
        private ParticleSystem explosionParticleSystem;
        
        public static void ShowWindow()
        {
            GetWindow<MeshFractureWindow>("Mesh Fracture Tool");
        }

        private void OnGUI()
        {
            GUILayout.Label("Mesh Fracture Settings", EditorStyles.boldLabel);

            // Select the object
            selectedObject = (GameObject)EditorGUILayout.ObjectField("Select Object", selectedObject, typeof(GameObject), true);

            if (selectedObject != null)
            {
                meshSuComponent = selectedObject.GetComponent<MeshSu>();

                if (meshSuComponent == null)
                {
                    GUILayout.Label("Selected object does not have the MeshSu component attached.");

                    // Provide an option to add the MeshSu component if missing
                    if (GUILayout.Button("Add MeshSu Component"))
                    {
                        // Add MeshSu component if it's missing
                        meshSuComponent = selectedObject.AddComponent<MeshSu>();
                        Debug.Log("MeshSu component added to " + selectedObject.name);
                    }

                    return;
                }

                // Display and edit fracture settings
                cutCascades = EditorGUILayout.IntField("Cut Cascades", cutCascades);
                combineAllFractures = EditorGUILayout.Toggle("Combine All Fractures", combineAllFractures);
                destroyMeshRightNow = EditorGUILayout.Toggle("Destroy Mesh Now", destroyMeshRightNow);
                destroyOnCollision = EditorGUILayout.Toggle("Destroy On Collision", destroyOnCollision);
                if (destroyOnCollision)
                {
                    neededCollisionForceToDestroy = EditorGUILayout.FloatField("Needed Collision Force", neededCollisionForceToDestroy);
                }
                explodeForce = EditorGUILayout.FloatField("Explosion Force", explodeForce);
                explosionParticleSystem = (ParticleSystem)EditorGUILayout.ObjectField("Explosion Particle System", explosionParticleSystem, typeof(ParticleSystem), false);

                // Apply the changes to the MeshSu component
                if (GUILayout.Button("Apply Fracture Settings"))
                {
                    meshSuComponent.CutCascades = cutCascades;
                    meshSuComponent.CombineAllFractures = combineAllFractures;
                    meshSuComponent.DestroyMeshRightNow = destroyMeshRightNow;
                    meshSuComponent.DestroyOnCollision = destroyOnCollision;
                    meshSuComponent.NeededCollisionForceToDestroy = neededCollisionForceToDestroy;
                    meshSuComponent.ExplodeForce = explodeForce;
                    meshSuComponent.ExplosionParticleSystem = explosionParticleSystem;

                    // Apply the changes
                    EditorUtility.SetDirty(meshSuComponent);
                }

                // Button to trigger the mesh destruction
                if (GUILayout.Button("Destroy Mesh Now"))
                {
                    if (meshSuComponent != null)
                    {
                        meshSuComponent.DestroyMesh();
                        Debug.Log("Mesh destroyed!");
                    }
                }

                // Button to delete empty objects after fracturing
                if (GUILayout.Button("Delete Fractures with 0 vertices"))
                {
                    DeleteEmptyObjects();
                }

                // Button to delete the original object
                if (GUILayout.Button("Delete Original Object"))
                {
                    DeleteOriginalObject();
                }
            }
            else
            {
                GUILayout.Label("Please select a GameObject to fracture.");
            }
        }

        private void DeleteEmptyObjects()
        {
            // Find all game objects in the scene
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

            foreach (var obj in allObjects)
            {
                // Check if the object has no mesh (i.e., it is empty)
                MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null && meshFilter.sharedMesh.vertexCount == 0)
                {
                    // If the mesh has no vertices, destroy the object
                    Debug.Log("Deleting empty object: " + obj.name);
                    DestroyImmediate(obj);
                }
            }
        }

        private void DeleteOriginalObject()
        {
            // Delete the original selected object
            if (selectedObject != null)
            {
                Debug.Log("Deleting original object: " + selectedObject.name);
                DestroyImmediate(selectedObject); // This will delete the object from the scene
            }
        }
    }

}

