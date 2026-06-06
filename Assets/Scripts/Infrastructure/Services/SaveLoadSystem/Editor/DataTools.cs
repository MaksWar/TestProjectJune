#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Infrastructure.Services.SaveLoadSystem.Editor
{
    public static class DataTools
    {        
        [MenuItem("Bugiko/DataTools/ClearSaveData")]
        private static void ClearSaveData()
        {
            string dir = Application.persistentDataPath;

            string[] files = Directory.GetFiles(dir, $"{SaveLoadService.SaveFilePrefix}*{SaveLoadService.SaveFileExtension}");
            foreach (string file in files)
            {
                try
                {
                    File.Delete(file);
                    
                    Debug.Log($"[SaveLoadTools] Deleted save file: {file}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[SaveLoadTools] Error deleting file {file}: {ex.Message}");
                }
            }
        }
        
        [MenuItem("Bugiko/DataTools/ClearAllLocalData")]
        private static void ClearAllLocalData()
        {
            PlayerPrefs.DeleteAll();
            var di = new DirectoryInfo(Application.persistentDataPath);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }

            foreach (var dir in di.GetDirectories())
            {
                dir.Delete(true);
            }

            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }
        }
    }
}
#endif