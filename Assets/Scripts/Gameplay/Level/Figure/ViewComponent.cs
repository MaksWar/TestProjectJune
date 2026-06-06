using UnityEngine;

namespace Gameplay.Level
{
    public class ViewComponent : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer view;
        
        public SpriteRenderer View => view;

        public void Initialize(Sprite sprite, Color color)
        {
            view.sprite = sprite;
            view.color = color;
        }
    }
}
