using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Infrastructure.Services.SaveLoadSystem.AuthService;
using UnityEngine;

namespace Infrastructure.Services.SaveLoadSystem
{
    public class SaveLoadManager : ISaveLoadManager
    {
        private readonly IAuthService _authService;
        private readonly ISaveLoadService _saveLoadService;
        private readonly IPrivateModelProvider _privateModelProvider;

        private bool _hasSaveData;
        private bool _processingSave;

        private const int TicksToSave = 200;

        public SaveLoadManager(IPrivateModelProvider privateModelProvider, ISaveLoadService saveLoadService, IAuthService authService)
        {
            _authService = authService;
            _saveLoadService = saveLoadService;
            _privateModelProvider = privateModelProvider;
        }

        public async UniTask InitializeAsync()
        {
            IPrivateModel.OnSave += UpdateSaveExisting;

            if (_authService.IsNewUser() == false)
            {
                Dictionary<string,object> data = await _saveLoadService.LoadAsync();
                
                await _privateModelProvider.MergeDataFromPull(data);
            }

            StartPeriodicalSave();
        }

        private async void StartPeriodicalSave()
        {
            while (true)
            {
                await UniTask.Delay(TicksToSave);
                await UniTask.WaitUntil(() => _processingSave == false && _hasSaveData);
                
                Debug.Log("Start Save Data");
                
                await SaveData();
                _hasSaveData = false;

                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }

        private async UniTask SaveData()
        {
            Dictionary<string, Dictionary<string, object>> data = _privateModelProvider.GetChangedDataToPush();
            if (data.Count == 0)
            {
                Debug.Log("Nothing to save");
                
                return;
            }

            _processingSave = true;

            Debug.Log($"Pushing data...");

            _privateModelProvider.ChangesSaved(data);

            await _saveLoadService.SaveAsync(data.ToDictionary(k => k.Key, v => (object)v.Value));

            _processingSave = false;
            Debug.Log($"Data pushed successfully");
        }

        private void UpdateSaveExisting() =>
            _hasSaveData = true;
    }
}