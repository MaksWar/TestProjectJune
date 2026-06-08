using UnityEngine;

namespace Enixan.GoldenDust.Utils
{
    public static class UIParticleExtension
    {
        public static void PreWarm(this ParticleSystem particleSystem)
        {
            particleSystem.gameObject.SetActive(false);
            particleSystem.gameObject.SetActive(true);
        }
        
        public static void PreWarmPlay(this ParticleSystem particleSystem)
        {
            particleSystem.gameObject.SetActive(false);
            particleSystem.gameObject.SetActive(true);
        }
        
        public static void PreWarmedStop(this ParticleSystem particleSystem) =>
            particleSystem.gameObject.SetActive(false);
    }
}