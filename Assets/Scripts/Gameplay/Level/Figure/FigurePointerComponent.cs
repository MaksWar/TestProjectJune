using UnityEngine;

namespace Gameplay.Level
{
    public class FigurePointerComponent : MonoBehaviour
    {
        [SerializeField] private PointerType figurePointerType;
        
        public PointerType FigurePointerType => figurePointerType;
    }
}
