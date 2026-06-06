using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Infrastructure.AssetManagement
{
    public class AssetModel
    {
        private readonly Dictionary<string, AssetData> _assetsData = new();

        public void AddAsset(string name, AsyncOperationHandle handle, Type user)
        {
            var users = new List<Type>(capacity: 1) { user };
            _assetsData.Add(name, new AssetData { Handle = handle, Users = users });
        }

        /// <summary>
        /// Добавление нового пользователя ассета
        /// </summary>
        /// <param name="name">Имя ассета</param>
        /// <param name="user">Тип пользователя</param>
        public void AddAssetUser(string name, Type user)
        {
            _assetsData[name].Users.Add(user);
        }

        /// <summary>
        /// Удаляем из списка пользователя ассета
        /// </summary>
        /// <param name="name">Имя ассета</param>
        /// <param name="user">Тип пользователя</param>
        /// <returns>Был ли это последний пользователь</returns>
        public bool RemoveAssetUser(string name, Type user)
        {
            _assetsData[name].Users.Remove(user);
            return _assetsData[name].Users.Count == 0;
        }

        public bool ContainsAsset(string name)
        {
            return _assetsData.ContainsKey(name);
        }

        public bool ContainsAssetUser(string name, Type user)
        {
            return _assetsData[name].Users.Contains(user);
        }

        public AsyncOperationHandle GetAssetHandle(string name)
        {
            return _assetsData[name].Handle;
        }

        public void RemoveAssetData(string name)
        {
            _assetsData.Remove(name);
        }

        /// <summary>
        /// Получения списка ассетов, которые никто не использует для выгрузки из памяти
        /// </summary>
        /// <returns></returns>
        public List<AsyncOperationHandle> GetAssetHandleWithoutInstances()
        {
            return _assetsData
                .Where(unit => unit.Value.Users.Count == 0)
                .Select(unit => unit.Value.Handle)
                .ToList();
        }

        /// <summary>
        /// Удаляем данные загруженых ассетов которые никто не использует
        /// Важно: не выгружает ассет из памяти
        /// </summary>
        public void RemoveDataWithoutUser()
        {
            var items = _assetsData
                .Where(unit => unit.Value.Users.Count == 0)
                .Select(unit => unit.Key)
                .ToList();

            foreach (var item in items)
            {
                RemoveAssetData(item);
            }
        }

        public bool IsPending(string key) =>
            _assetsData[key].IsPending;

        /// <summary>
        /// Класс для хранения загруженых ассетов
        /// </summary>
        private class AssetData
        {
            /// <summary>
            /// Хендлер загруженого ассета
            /// </summary>
            public AsyncOperationHandle Handle;
            /// <summary>
            /// Список типов которые брали себе копия загруженого ассета
            /// </summary>
            public List<Type> Users;
            
            /// <summary>
            /// Проверка на то, что ассет загружается в данный момент
            /// </summary>
            public bool IsPending => Handle.IsValid() && Handle.IsDone == false;
        }
    }
}