using System.Collections.Generic;
using UnityEngine;

namespace Gitmanik.FOV2D
{
    public class FOVMesh : MonoBehaviour
    {
        [Header("Configuration")]
        public FOV fov;
        [Tooltip("1 for one step every angle")] [Range(0,5)] [SerializeField] private float meshResolution;
        
        private Mesh mesh;
        
        private Vector3[] vertices;
        private int[] triangles;
        private int stepCount;
        private float stepAngle;
        private List<Vector3> viewVertex = new List<Vector3>();

        public void Setup()
        {
            stepCount = Mathf.RoundToInt(fov.viewAngle * meshResolution);
            stepAngle = fov.viewAngle / stepCount;
        }

        void Start()
        {
            mesh = GetComponent<MeshFilter>().mesh;
            Setup();
        }

        public void UpdateMesh()
        {
            viewVertex.Clear();

            for (int i = 0; i <= stepCount; i++)
            {
                float angle = fov.transform.eulerAngles.y - fov.viewAngle / 2 + stepAngle * i;
                Vector3 dir = fov.DirFromAngle(angle, false);
                RaycastHit2D hit = Physics2D.Raycast(fov.transform.position, dir, fov.viewRadius, fov.obstacleMask);

                if (hit.collider == null)
                    viewVertex.Add(transform.position + dir.normalized * fov.viewRadius);
                else
                    viewVertex.Add(transform.position + dir.normalized * hit.distance);
            }

            int vertexCount = viewVertex.Count + 1;
            vertices = new Vector3[vertexCount];
            triangles = new int[(vertexCount - 2) * 3];
            vertices[0] = Vector3.zero;

            for (int i = 0; i < vertexCount - 1; i++)
            {
                vertices[i + 1] = transform.InverseTransformPoint(viewVertex[i]);

                if (i < vertexCount - 2)
                {
                    triangles[i * 3 + 2] = 0;
                    triangles[i * 3 + 1] = i + 1;
                    triangles[i * 3] = i + 2;
                }
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
        }
    }
}