using Infrastructure.Services.Log;
using Infrastructure.States;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Infrastructure.UI.Elements
{
    public class GameStateSwitchButton : MonoBehaviour
    {
        private enum TargetStates
        {
            None = 0,
            Loading = 1,
            Gameplay = 2 ,
            GenerateLevel = 3 ,
        }
        
        [SerializeField] private TargetStates targetState = 0;
        [SerializeField] private Button button;

        private GameStateMachine gameStateMachine;
        private ILogService log;

        [Inject]
        void Construct(GameStateMachine gameStateMachine, ILogService log)
        {
            this.gameStateMachine = gameStateMachine;
            this.log = log;
        }

        private void OnEnable() => 
            button.onClick.AddListener(OnClick);

        private void OnDisable() => 
            button.onClick.RemoveListener(OnClick);

        private void OnClick()
        {
            switch (targetState)
            {
                case TargetStates.Loading: gameStateMachine.Enter<GameLoadDataState>(); break;
                case TargetStates.Gameplay: gameStateMachine.Enter<GameplayState, string>("polka"); break;
                default: log.LogError("Not valid option"); break;
            }
        }
    }
}
