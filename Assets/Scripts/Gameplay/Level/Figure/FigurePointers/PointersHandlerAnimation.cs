using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Level
{
    public class PointersHandlerAnimationComponent : MonoBehaviour
    {
        [Header("Setings")]
        [SerializeField] private float pathShowDuration = 1f;

        public async UniTask ShowCurrentPath(IReadOnlyList<FigurePointerComponent> pointers)
        {
            if (pointers == null || pointers.Count == 0)
            {
                return;
            }

            var tasks = new List<UniTask>();
            foreach (FigurePointerComponent pointer in pointers)
            {
                tasks.Add(pointer.Show(pathShowDuration));
            }
            
            await UniTask.WhenAll(tasks);
        }
    }
}
