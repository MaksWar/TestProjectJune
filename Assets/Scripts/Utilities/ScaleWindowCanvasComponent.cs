using UnityEngine;
using UnityEngine.UI;

namespace Utilities
{
    public class ScaleWindowCanvasComponent : MonoBehaviour
    {
        private float _relation = 1.77f;

        void Start()
        {
            Rect safeArea = Screen.safeArea;
            _relation = safeArea.width / safeArea.height;

            if (_relation >= 1.77f)
            {
                SetScale(0.9f);
            }
            else if (_relation >= 1.45f && _relation < 1.77f)
            {
                SetScale(0.7f);
            }
            else if (_relation < 1.45f)
            {
                SetScale(1f);
            }
        }

        private void SetScale(float value)
        {
            var panel = GetComponent<CanvasScaler>();
            panel.matchWidthOrHeight = value;
        }
    }
}