using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.LevelMenu
{
    public class CategoriesGroupViewComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI categoryNameText;
        [SerializeField] private Transform levelViewContainer;
        [SerializeField] private NestedScrollRectDirectionRouter scrollDirectionRouter;
        
        public Transform LevelViewContainer => levelViewContainer;

        public void Initialize(string categoryName) =>
            categoryNameText.text = categoryName;

        public void SetParentScrollRect(ScrollRect scrollRect) =>
            scrollDirectionRouter.SetParentScrollRect(scrollRect);
    }
}
