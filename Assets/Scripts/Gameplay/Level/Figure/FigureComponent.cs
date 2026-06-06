using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Level
{
    public class FigureComponent : MonoBehaviour
    {
        [SerializeField] private List<PathComponent> paths;
        [SerializeField] private ViewComponent view;
        
        public List<PathComponent> Paths => paths;
        public ViewComponent View => view;
    }
}