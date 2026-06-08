using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Infrastructure.Services.SaveLoadSystem
{
    public class SaveLoadService : ISaveLoadService
    {
        public const string SaveFilePrefix = "save_data_";
        public const string SaveFileExtension = "json";

        public async UniTask SaveAsync(Dictionary<string, object> data, CancellationToken cancellationToken = default)
        {
            foreach (KeyValuePair<string, object> pair in data)
            {
                string dataName = pair.Key;
                object dataValue = pair.Value;

                try
                {
                    string json = JsonConvert.SerializeObject(dataValue, Formatting.Indented);
                    string path = GetSavePath(dataName);

                    await File.WriteAllTextAsync(path, json, cancellationToken);

                    Debug.Log($"[SaveLoadService] Data Saved {dataName} to {path}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[SaveLoadService] Error during save: {ex.Message}");
                }
            }
        }

        public async UniTask<Dictionary<string, object>> LoadAsync(CancellationToken cancellationToken = default)
        {
            var result = new Dictionary<string, object>();
            string dir = Application.persistentDataPath;

            string[] files = Directory.GetFiles(dir, $"{SaveFilePrefix}*{SaveFileExtension}");
            foreach (string path in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                var dataName = fileName.Substring(SaveFilePrefix.Length);

                try
                {
                    string json = await File.ReadAllTextAsync(path, cancellationToken);
                    object model = JsonConvert.DeserializeObject<object>(json);

                    result[dataName] = model;

                    Debug.Log($"[SaveLoadService] Loaded '{dataName}' from {path}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SaveLoadService] Error loading '{dataName}': {ex}");
                }
            }

            return result;
        }

        private string GetSavePath(string dataName) =>
            Path.Combine(Application.persistentDataPath, $"{SaveFilePrefix}{dataName}.{SaveFileExtension}");
    }
}