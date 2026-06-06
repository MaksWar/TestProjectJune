using System.Collections;
using Cysharp.Threading.Tasks;
using Enixan.Engine.UI.Windows;
using UnityEngine;
using Zenject;

namespace Infrastructure.UI.LoadingCurtain
{
    public class LoadingCurtain : MonoBehaviour, ILoadingCurtain
    {
        [SerializeField] private CanvasGroup canvas;
        [SerializeField] private LoadingTextAnimationComponent loadingTextAnimationComponent;
        
        public void Show()
        {
            gameObject.SetActive(true);
            canvas.alpha = 1;
            
            loadingTextAnimationComponent.StartAnimation();
        }

        public void Hide()
        {
            if (gameObject.activeSelf == false)
            {
                return;
            }
            
            StartCoroutine(DoFadeIn());
        }

        private IEnumerator DoFadeIn()
        {
            while (canvas.alpha > 0)
            {
                canvas.alpha -= 0.06f;
                yield return new WaitForSeconds(0.03f);
            }

            loadingTextAnimationComponent.StopAnimation();
            gameObject.SetActive(false);
        }
        
        public class Factory : PlaceholderFactory<string, UniTask<LoadingCurtain>>
        {
        }
    }
}