using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Infrastructure.Services.SaveLoadSystem.AuthService;
using UnityEngine;
using Utilities;
using Zenject;

namespace Infrastructure.Services.SaveLoadSystem
{
    public class PrivateModelProvider : IPrivateModelProvider
    {
        private readonly DiContainer _diContainer;
        private readonly IAuthService _authService;

        private List<IPrivateModel> _dataModels;

        private bool _isSetup;
        private bool _dataPulled;

        public PrivateModelProvider(DiContainer diContainer, IAuthService authService)
        {
            _diContainer = diContainer;
            _authService = authService;
        }

        public async UniTask InitializeAsync()
        {
            _dataModels = _diContainer.ResolveAll<IPrivateModel>();
            if(IsModelsExist() == false)
            {
                Debug.LogError("PrivateModelProvider: No data models found.");
             
                return;
            }

            // TODO : add check for new user
            if (_authService.IsNewUser())
            {
                Debug.Log("FirstSync | PrivateModelProvider | InitializeAsync | IsNewUser");

                SetupAll();
            }
        }

        public async UniTask MergeDataFromPull(Dictionary<string, object> data)
        {
            Debug.Log("MergePrivateData Start");

            if ((data == null || data.Count == 0) && _isSetup == false)
            {
                SetupAll();
            }
            else
            {
                InitAll();
            }
            
            if (data != null)
            {
                foreach (KeyValuePair<string, object> item in data)
                {
                    int index = _dataModels.FindIndex(x => x.DataName == item.Key);
                    if (index != -1)
                    {
                        try
                        {
                            await _dataModels[index].Merge(item.Value.ToObject<Dictionary<string, object>>());
                        }
                        catch (Exception error)
                        {
                            throw new Exception($"Merge model {item.Key} fail with: {error} StackTrace: {error.StackTrace}");
                        }
                    }
                }
            }

            _dataPulled = true;
            
            Debug.Log("MergePrivateData End");
        }

        public Dictionary<string, Dictionary<string, object>> GetChangedDataToPush(List<string> filter = null)
        {
            var result = new Dictionary<string, Dictionary<string, object>>();
            foreach (var dataModel in _dataModels)
            {
                Dictionary<string, object> chahges = dataModel.GetChanges();
                if (chahges == null || chahges.Count == 0)
                {
                    continue;
                }

                if (filter == null || filter.Count == 0 || filter.Contains(dataModel.DataName))
                {
                    result.Add(dataModel.DataName, chahges);
                }
            }

            return result;
        }

        public T Get<T>() where T : class, IPrivateModel
        {
            if (_dataModels == null)
            {
                Debug.LogError("Get<T> failed: _dataModels is null.");
                
                return null;
            }

            var result = _dataModels.Find(x => x is T) as T;
            if (result == null)
            {
                Debug.LogWarning($"Get<T> did not find an instance of type {typeof(T).Name} in _dataModels.");
            }

            return result;
        }

        public void Set<T>(T item) where T : class, IPrivateModel
        {
            int index = _dataModels.FindIndex(x => x is T);
            if (index == -1)
            {
                _dataModels.Add(item);
            }
            else
            {
                _dataModels[index] = item;
            }
        }

        public void ChangesSaved(Dictionary<string, Dictionary<string, object>> data)
        {
            foreach (IPrivateModel dataModel in _dataModels)
            {
                if (data.ContainsKey(dataModel.DataName))
                {
                    dataModel.ChangesSaved(data[dataModel.DataName].Keys.ToList());
                }
            }
        }

        public List<T> GetAll<T>() where T : class =>
            _dataModels.Where(x => x is T).OfType<T>().ToList();

        public IPrivateModel Get(string dataName) =>
            _dataModels.Find(x => x.DataName == dataName);

        /// <summary>
        /// Wrapped method for <see cref="IPrivateModel.Initialize"/>
        /// </summary>
        /// <param name="dataModel"></param>
        private void InitModel(IPrivateModel dataModel)
        {
            try
            {
                dataModel.Initialize();
            }
            catch (Exception e)
            {
                Debug.LogError($"Initializing model {dataModel.DataName} fail. {e.Message} \n {e.StackTrace}");
            }
        }

        /// <summary>
        /// Делаем первоначальное наполнение моделей.
        /// </summary>
        private void SetupAll()
        {
            foreach (var dataModel in _dataModels)
            {
                dataModel.Setup();
            }

            _isSetup = true;
        }

        private void InitAll()
        {
            foreach (IPrivateModel model in _dataModels)
            {
                InitModel(model);
            }
        }

        private bool IsModelsExist() =>
            _dataModels is { Count: > 0 };
    }
}