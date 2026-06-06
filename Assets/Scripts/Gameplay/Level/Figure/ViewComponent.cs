using UnityEngine;

namespace Gameplay.Level
{
    public class ViewComponent : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer view;
        
        public SpriteRenderer View => view;
    }
}