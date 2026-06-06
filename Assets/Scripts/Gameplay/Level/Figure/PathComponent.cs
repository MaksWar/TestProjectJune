using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Level
{
    public class PathComponent : MonoBehaviour
    {
        [SerializeField] private List<PathPointComponent> points;
        
        public List<PathPointComponent> Points => points;
    }
}