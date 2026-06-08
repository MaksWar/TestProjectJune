using System;
using Gameplay.Level;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.LevelMenu
{
    public class LevelViewComponent : MonoBehaviour
    {
        public event Action<FigureType, string> OnClick;
        
        [SerializeField] private Button button;
        [SerializeField] private Image view;

        private FigureType _type;
        private string _id;

        public void Initialize(FigureType type, string id, Sprite sprite, Color color)
        {
            _type = type;
            _id = id;
            
            view.sprite = sprite;
            view.color = color;

            button.onClick.RemoveListener(HandleClick);
            button.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
            }
        }

        private void HandleClick()
        {
            OnClick?.Invoke(_type, _id);
        }
    }
}
