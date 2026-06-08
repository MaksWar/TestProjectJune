using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Gameplay.Tips.GameplayTips;

namespace Gameplay.Tips
{
    public class GameplayTipTimer
    {
        private readonly Dictionary<float, BaseGameplayTip> _tipsByInactivityTime;
        private readonly HashSet<float> _playedTimes = new();
        private readonly List<float> _sortedTimes;

        private float _inactiveTime;
        private bool _isRunning;

        public GameplayTipTimer(Dictionary<float, BaseGameplayTip> tipsByInactivityTime)
        {
            _tipsByInactivityTime = tipsByInactivityTime;
            _sortedTimes = tipsByInactivityTime.Keys.OrderBy(time => time).ToList();
        }

        public void Start()
        {
            _inactiveTime = 0f;
            _playedTimes.Clear();
            _isRunning = true;
        }

        public void Reset()
        {
            _inactiveTime = 0f;
            _playedTimes.Clear();
        }

        public void Stop()
        {
            _isRunning = false;
            Reset();
        }

        public void Tick(float deltaTime, GameplayTipContext context)
        {
            if (!_isRunning)
            {
                return;
            }

            _inactiveTime += deltaTime;

            foreach (float inactivityTime in _sortedTimes)
            {
                if (_inactiveTime < inactivityTime || _playedTimes.Contains(inactivityTime))
                {
                    continue;
                }

                _playedTimes.Add(inactivityTime);
                _tipsByInactivityTime[inactivityTime].Play(context).Forget();
            }
        }
    }
}
