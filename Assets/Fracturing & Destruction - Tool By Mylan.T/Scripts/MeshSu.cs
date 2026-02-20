using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MeshSuFracturer
{
    public class MeshSu : MonoBehaviour
    {

        [Header("FRACTURES SETTINGS")]
        [Space(20)]
        [Tooltip("how many times slice object")] public int CutCascades = 1;
        [Tooltip("create parent for all fractures")] public bool CombineAllFractures = true;
        [Space(10)]
        [Header("destroy method:")]
        [Tooltip("Destroying object when bool is true")] public bool DestroyMeshRightNow = false;
        [Space(5)]
        [Tooltip("destroy mesh on collision enter void")] public bool DestroyOnCollision = false;
        [Tooltip("needed force collision to break object")] public float NeededCollisionForceToDestroy = 0;
        [Space(10)]
        [Header("explosion:")]
        [Tooltip("explosion force")] public float ExplodeForce = 0;
        [Tooltip("Explosion Particles")] public ParticleSystem ExplosionParticleSystem;






        void Update()
        {
            if (DestroyMeshRightNow == true && alreadyDonr == false)
            {
                alreadyDonr = true;
                DestroyMesh();
            }
        }
        private bool edgeSet = false;
        private Vector3 edgeVertex = Vector3.zero;
        private Vector2 edgeUV = Vector2.zero;
        private Plane edgePlane = new Plane();
        private bool alreadyDonr;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.relativeVelocity.magnitude >= NeededCollisionForceToDestroy && DestroyOnCollision == true)
            {
                DestroyMesh();
            }
        }


        public void DestroyMesh()
        {
            if (ExplosionParticleSystem != null)
            {
                Instantiate(ExplosionParticleSystem, transform.position, Quaternion.Euler(0, 0, 0));
            }
            var originalMesh = GetComponent<MeshFilter>().mesh;
            originalMesh.RecalculateBounds();
            var parts = new List<PartMesh>();
            var subParts = new List<PartMesh>();

            var mainPart = new PartMesh()
            {
                UV = originalMesh.uv,
                Vertices = originalMesh.vertices,
                Normals = originalMesh.normals,
                Triangles = new int[originalMesh.subMeshCount][],
                Bounds = originalMesh.bounds
            };

            for (int i = 0; i < originalMesh.subMeshCount; i++)
                mainPart.Triangles[i] = originalMesh.GetTriangles(i);

            parts.Add(mainPart);


            if (CombineAllFractures)
            {

                GameObject parentObject = new GameObject("Destroyed " + originalMesh.name + " Parts");
                parentObject.transform.position = transform.position;
                FracturesParent = parentObject;

            }


            for (var c = 0; c < CutCascades; c++)
            {
                for (var i = 0; i < parts.Count; i++)
                {
                    var bounds = parts[i].Bounds;
                    bounds.Expand(0.5f);

                    var plane = new Plane(UnityEngine.Random.onUnitSphere, new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                                                                                        UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
                                                                                        UnityEngine.Random.Range(bounds.min.z, bounds.max.z)));

                    subParts.Add(GenerateMesh(parts[i], plane, true));
                    subParts.Add(GenerateMesh(parts[i], plane, false));
                }
                parts = new List<PartMesh>(subParts);
                subParts.Clear();
            }

            for (var i = 0; i < parts.Count; i++)
            {
                parts[i].MakeGameobject(this);

                if (CombineAllFractures)
                {
                    parts[i].GameObject.transform.parent = FracturesParent.transform;
                }


                parts[i].GameObject.GetComponent<Rigidbody>().AddForceAtPosition(parts[i].Bounds.center * ExplodeForce, transform.position);
            }
            Destroy(gameObject);


        }
        private GameObject FracturesParent;

        private PartMesh GenerateMesh(PartMesh original, Plane plane, bool left)
        {    // Initialize a new part mesh to hold the final mesh
            var partMesh = new PartMesh() { };
            var ray1 = new Ray();// Ray used for calculating intersections
            var ray2 = new Ray();// Another ray for the second intersection check


            for (var i = 0; i < original.Triangles.Length; i++)
            {
                var triangles = original.Triangles[i];
                edgeSet = false;// Flag to track if an edge has been set for processing
                                // Iterate through each triangle (3 vertices per triangle)
                for (var j = 0; j < triangles.Length; j = j + 3)
                {   // Check on which side of the plane each vertex lies
                    var sideA = plane.GetSide(original.Vertices[triangles[j]]) == left;
                    var sideB = plane.GetSide(original.Vertices[triangles[j + 1]]) == left;
                    var sideC = plane.GetSide(original.Vertices[triangles[j + 2]]) == left;
                    // Count how many vertices lie on the "left" side of the plane
                    var sideCount = (sideA ? 1 : 0) +
                                    (sideB ? 1 : 0) +
                                    (sideC ? 1 : 0);
                    if (sideCount == 0)      // If no vertices are on the left side, skip this triangle
                    {
                        continue;
                    }
                    if (sideCount == 3) // If all vertices are on the left side, add the triangle to the mesh without modification
                    {
                        partMesh.AddTriangle(i,
                                             original.Vertices[triangles[j]], original.Vertices[triangles[j + 1]], original.Vertices[triangles[j + 2]],
                                             original.Normals[triangles[j]], original.Normals[triangles[j + 1]], original.Normals[triangles[j + 2]],
                                             original.UV[triangles[j]], original.UV[triangles[j + 1]], original.UV[triangles[j + 2]]);
                        continue;
                    }
                    // Determine which single vertex is different (not on the same side as the other two)
                    var singleIndex = sideB == sideC ? 0 : sideA == sideC ? 1 : 2;
                    // Set up the first ray for intersection calculation
                    ray1.origin = original.Vertices[triangles[j + singleIndex]];
                    var dir1 = original.Vertices[triangles[j + ((singleIndex + 1) % 3)]] - original.Vertices[triangles[j + singleIndex]];
                    ray1.direction = dir1;
                    plane.Raycast(ray1, out var enter1);
                    var lerp1 = enter1 / dir1.magnitude;
                    // Set up the second ray for intersection calculation
                    ray2.origin = original.Vertices[triangles[j + singleIndex]];
                    var dir2 = original.Vertices[triangles[j + ((singleIndex + 2) % 3)]] - original.Vertices[triangles[j + singleIndex]];
                    ray2.direction = dir2;
                    plane.Raycast(ray2, out var enter2);
                    var lerp2 = enter2 / dir2.magnitude;

                    AddEdge(i,
                            partMesh,
                            left ? plane.normal * -1f : plane.normal,
                            ray1.origin + ray1.direction.normalized * enter1,
                            ray2.origin + ray2.direction.normalized * enter2,
                            Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                            Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 2) % 3)]], lerp2));

                    if (sideCount == 1)
                    {
                        partMesh.AddTriangle(i,
                                            original.Vertices[triangles[j + singleIndex]],
                                            //Vector3.Lerp(originalMesh.vertices[triangles[j + singleIndex]], originalMesh.vertices[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                                            //Vector3.Lerp(originalMesh.vertices[triangles[j + singleIndex]], originalMesh.vertices[triangles[j + ((singleIndex + 2) % 3)]], lerp2),
                                            ray1.origin + ray1.direction.normalized * enter1,
                                            ray2.origin + ray2.direction.normalized * enter2,
                                            original.Normals[triangles[j + singleIndex]],
                                            Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                                            Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 2) % 3)]], lerp2),
                                            original.UV[triangles[j + singleIndex]],
                                            Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                                            Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 2) % 3)]], lerp2));

                        continue;
                    }

                    if (sideCount == 2)
                    {
                        partMesh.AddTriangle(i,
                                            ray1.origin + ray1.direction.normalized * enter1,
                                            original.Vertices[triangles[j + ((singleIndex + 1) % 3)]],
                                            original.Vertices[triangles[j + ((singleIndex + 2) % 3)]],
                                            Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                                            original.Normals[triangles[j + ((singleIndex + 1) % 3)]],
                                            original.Normals[triangles[j + ((singleIndex + 2) % 3)]],
                                            Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                                            original.UV[triangles[j + ((singleIndex + 1) % 3)]],
                                            original.UV[triangles[j + ((singleIndex + 2) % 3)]]);
                        partMesh.AddTriangle(i,
                                            ray1.origin + ray1.direction.normalized * enter1,
                                            original.Vertices[triangles[j + ((singleIndex + 2) % 3)]],
                                            ray2.origin + ray2.direction.normalized * enter2,
                                            Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                                            original.Normals[triangles[j + ((singleIndex + 2) % 3)]],
                                            Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 2) % 3)]], lerp2),
                                            Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                                            original.UV[triangles[j + ((singleIndex + 2) % 3)]],
                                            Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 2) % 3)]], lerp2));
                        continue;
                    }


                }
            }

            partMesh.FillArrays();

            return partMesh;
        }

        private void AddEdge(int subMesh, PartMesh partMesh, Vector3 normal, Vector3 vertex1, Vector3 vertex2, Vector2 uv1, Vector2 uv2)
        {
            if (!edgeSet)
            {
                edgeSet = true;
                edgeVertex = vertex1;
                edgeUV = uv1;
            }
            else
            {
                edgePlane.Set3Points(edgeVertex, vertex1, vertex2);

                partMesh.AddTriangle(subMesh,
                                    edgeVertex,
                                    edgePlane.GetSide(edgeVertex + normal) ? vertex1 : vertex2,
                                    edgePlane.GetSide(edgeVertex + normal) ? vertex2 : vertex1,
                                    normal,
                                    normal,
                                    normal,
                                    edgeUV,
                                    uv1,
                                    uv2);
            }
        }

        public class PartMesh
        {


            private List<Vector3> _Verticies = new List<Vector3>();
            private List<Vector3> _Normals = new List<Vector3>();
            private List<List<int>> _Triangles = new List<List<int>>();
            private List<Vector2> _UVs = new List<Vector2>();
            public Vector3[] Vertices;
            public Vector3[] Normals;
            public int[][] Triangles;
            public Vector2[] UV;
            public GameObject GameObject;
            public Bounds Bounds = new Bounds();

            public PartMesh()
            {

            }

            public void AddTriangle(int submesh, Vector3 vert1, Vector3 vert2, Vector3 vert3, Vector3 normal1, Vector3 normal2, Vector3 normal3, Vector2 uv1, Vector2 uv2, Vector2 uv3)
            {
                if (_Triangles.Count - 1 < submesh)
                    _Triangles.Add(new List<int>());

                _Triangles[submesh].Add(_Verticies.Count);
                _Verticies.Add(vert1);
                _Triangles[submesh].Add(_Verticies.Count);
                _Verticies.Add(vert2);
                _Triangles[submesh].Add(_Verticies.Count);
                _Verticies.Add(vert3);
                _Normals.Add(normal1);
                _Normals.Add(normal2);
                _Normals.Add(normal3);
                _UVs.Add(uv1);
                _UVs.Add(uv2);
                _UVs.Add(uv3);

                Bounds.min = Vector3.Min(Bounds.min, vert1);
                Bounds.min = Vector3.Min(Bounds.min, vert2);
                Bounds.min = Vector3.Min(Bounds.min, vert3);
                Bounds.max = Vector3.Min(Bounds.max, vert1);
                Bounds.max = Vector3.Min(Bounds.max, vert2);
                Bounds.max = Vector3.Min(Bounds.max, vert3);
            }

            public void FillArrays()
            {
                Vertices = _Verticies.ToArray();
                Normals = _Normals.ToArray();
                UV = _UVs.ToArray();
                Triangles = new int[_Triangles.Count][];
                for (var i = 0; i < _Triangles.Count; i++)
                    Triangles[i] = _Triangles[i].ToArray();
            }

            public void MakeGameobject(MeshSu original)
            {

                GameObject = new GameObject(original.name + " (Fracture)");
                GameObject.transform.position = original.transform.position;
                GameObject.transform.rotation = original.transform.rotation;
                GameObject.transform.localScale = original.transform.localScale;

                var mesh = new Mesh();
                mesh.name = original.GetComponent<MeshFilter>().mesh.name;

                mesh.vertices = Vertices;
                mesh.normals = Normals;
                mesh.uv = UV;
                for (var i = 0; i < Triangles.Length; i++)
                    mesh.SetTriangles(Triangles[i], i, true);
                Bounds = mesh.bounds;

                var renderer = GameObject.AddComponent<MeshRenderer>();
                renderer.materials = original.GetComponent<MeshRenderer>().materials;

                var filter = GameObject.AddComponent<MeshFilter>();
                filter.mesh = mesh;

                var collider = GameObject.AddComponent<MeshCollider>();
                collider.convex = true;

                var rigidbody = GameObject.AddComponent<Rigidbody>();
                // var meshDestroy = GameObject.AddComponent<MeshSu>();
                //  meshDestroy.CutCascades = original.CutCascades;
                //  meshDestroy.ExplodeForce = original.ExplodeForce;
                if (filter.mesh.vertexCount == 0)
                {

                    Destroy(filter.gameObject);



                }


            }

        }
    }

}
