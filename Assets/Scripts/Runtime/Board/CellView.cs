using UnityEngine;

namespace Game.Runtime
{
    public sealed class CellView : MonoBehaviour
    {
        [SerializeField] private MeshFilter _mesh;

        public void SetMesh(Mesh mesh)
        {
            _mesh.sharedMesh = mesh;
        }
       
        public void Place(float x, float y, float z)
        {
            transform.localPosition = new Vector3(x, y, z);
        }

        public void Resize(float scale)
        {
            transform.localScale = new Vector3(scale, scale, scale);
        }

        public void Face(Quaternion rotation)
        {
            transform.localRotation = rotation;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void Configure(MeshFilter mesh)
        {
            _mesh = mesh;
        }
#endif
    }
}
