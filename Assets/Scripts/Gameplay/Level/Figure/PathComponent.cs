using System.Collections.Generic;
using Gameplay.Level.Models.Public;
using UnityEngine;

namespace Gameplay.Level
{
    public class PathComponent : MonoBehaviour
    {
        [SerializeField] private int order;
        [SerializeField] private PathEntryType type;
        [SerializeField] private bool closed;
        [SerializeField] private List<Vector2> path;
        [SerializeField] private List<PathPointComponent> points;
        
        public int Order => order;
        public PathEntryType Type => type;
        public bool Closed => closed;
        public List<Vector2> Path => path;
        public List<PathPointComponent> Points => points;

        public void Initialize(PathEntry pathEntry, List<PathPointComponent> pointComponents)
        {
            order = pathEntry.Order;
            type = pathEntry.Type;
            closed = pathEntry.Closed;
            path = pathEntry.Path ?? new List<Vector2>();
            points = pointComponents;
        }
    }
}
