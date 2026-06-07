using System;
using UnityEngine;

namespace Infrastructure.Gameplay.Tips
{
    [Serializable]
    public class GameplayTipSettings
    {
        [SerializeField] private float fingerPointsPerSecond = 3f;
        [SerializeField] private Vector3 fingerOffset = new(0.25f, -0.25f, 0f);

        public float FingerPointsPerSecond => Mathf.Max(0.01f, fingerPointsPerSecond);
        public Vector3 FingerOffset => fingerOffset;
    }
}
