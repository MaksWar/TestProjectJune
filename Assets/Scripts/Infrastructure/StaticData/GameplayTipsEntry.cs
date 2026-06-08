using UnityEngine;

namespace Infrastructure.StaticData
{
    [CreateAssetMenu(fileName = "GameplayTipsEntry", menuName = "Static Data/Gameplay Tips Entry")]
    public class GameplayTipsEntry : ScriptableObject
    {
        [SerializeField] private float soundTipInactiveTime = 7f;
        [SerializeField] private float fingerTipInactiveTime = 14f;

        public float SoundTipInactiveTime => soundTipInactiveTime;
        public float FingerTipInactiveTime => fingerTipInactiveTime;
    }
}
