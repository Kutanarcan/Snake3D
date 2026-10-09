using System;
using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    // One row of the table that turns what a cell is made of into what it looks
    // like. A new ground type costs one row in the Inspector and no code.
    [Serializable]
    public sealed class GroundVisual
    {
        [SerializeField] private CellGroundType _type;
        [SerializeField] private Mesh _mesh;
        [SerializeField] private float _height = 1f;

        public CellGroundType Type { get { return _type; } }
        public Mesh Mesh { get { return _mesh; } }
        public float Height { get { return _height; } }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public GroundVisual()
        {
        }

        public GroundVisual(CellGroundType type, Mesh mesh, float height)
        {
            _type = type;
            _mesh = mesh;
            _height = height;
        }
#endif
    }
}
