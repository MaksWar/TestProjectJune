using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Infrastructure.Services.SaveLoadSystem
{
    public abstract class BasePrivateModelScheme : IPrivateModelScheme
    {
        protected List<string> DirtyKeys = new List<string>();

        private const string DirtyKeysFieldKey = "DirtyKeys";

        public virtual Dictionary<string, object> GetDirtyData()
        {
            Dictionary<string, object> data = Serialize();

            data = data
                .Where(item => item.Key != DirtyKeysFieldKey/*DirtyKeys.Contains(item.Key)*/) // TODO: Відправляємо всі поля, так як сейвимо локально на пристрій
                .ToDictionary(k => k.Key, v => v.Value);

            return data;
        }

        public void AddDirtyKey(string key)
        {
            if (DirtyKeys.Contains(key))
            {
                return;
            }

            DirtyKeys.Add(key);
        }

        public Dictionary<string, object> Serialize()
        {
            Dictionary<string, object> data = SerializeProperties();
            if (DirtyKeys != null && DirtyKeys.Count > 0)
            {
                data.Add("DirtyKeys", DirtyKeys);
            }
            
            return data;
        }

        public void OnPushFail(List<string> failedKeys)
        {
            foreach (string dirtyKey in failedKeys)
            {
                AddDirtyKey(dirtyKey);
            }
        }

        public void Deserialize(Dictionary<string, object> data)
        {
            DeserializeProperties(data);

            DirtyKeys = GetValue<List<string>>("DirtyKeys", data) ?? new List<string>();
        }

        public void RemoveDirty(List<string> keys) =>
            DirtyKeys.RemoveAll(keys.Contains);

        public void ClearDirtyKeys() =>
            DirtyKeys.Clear();

        protected T GetValue<T>(string key, IDictionary<string, object> data)
        {
            if (data == null)
            {
                Debug.LogError($"Data is null. {GetType()}");

                return default;
            }

            if (data.TryGetValue(key, out object obj) == false)
            {
                return default;
            }

            if (obj == null)
            {
                return default;
            }

            if (obj is JObject jObject)
            {
                return jObject.ToObject<T>();
            }

            if (obj is JArray jArray)
            {
                return jArray.ToObject<T>();
            }

            return (T) Convert.ChangeType(obj, typeof(T));
        }

        protected abstract void DeserializeProperties(Dictionary<string, object> data);

        protected abstract Dictionary<string, object> SerializeProperties();
    }
}