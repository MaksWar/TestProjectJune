using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Enixan.Engine.UI.Windows
{
    public class LoadingTextAnimationComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI loadingText; 
        [SerializeField] private float interval = 0.5f;

        private int dotCount;
        private bool isAnimating = true;

        private readonly string baseText = "Loading";

        public void StartAnimation()
        {
            isAnimating = true;
            AnimateDotsAsync().Forget();
        }

        public void StopAnimation()
        {
            isAnimating = false;
            loadingText.text = baseText;
        }

        private async UniTaskVoid AnimateDotsAsync()
        {
            while (isAnimating)
            {
                dotCount = (dotCount + 1) % 4;
                loadingText.text = baseText + new string('.', dotCount);

                await UniTask.Delay((int)(interval * 1000));
            }
        }
    }
}