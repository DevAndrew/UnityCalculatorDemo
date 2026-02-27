using System;
using System.IO;
using DevAndrew.SaveLoad.Contracts;
using UnityEngine;

namespace DevAndrew.SaveLoad.Infrastructure
{
    public sealed class JsonSaveLoadService : ISaveLoadService
    {
        private const string BackupSuffix = ".bak";
        private const string TempSuffix = ".tmp";

        private readonly string _storageDirectory;

        public JsonSaveLoadService()
            : this(Application.persistentDataPath)
        {
        }

        public JsonSaveLoadService(string storageDirectory)
        {
            _storageDirectory = string.IsNullOrWhiteSpace(storageDirectory)
                ? Application.persistentDataPath
                : storageDirectory;
        }

        public bool TryLoad<T>(string fileName, out T data) where T : class
        {
            data = null;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                Debug.LogError("JsonSaveLoadService: fileName is empty.");
                return false;
            }

            var statePath = BuildPath(fileName);
            var backupPath = statePath + BackupSuffix;

            if (TryLoadFromPath(statePath, out data))
            {
                return true;
            }

            if (TryLoadFromPath(backupPath, out data))
            {
                Debug.LogWarning($"Save load fallback: using backup file {backupPath}");
                return true;
            }

            return false;
        }

        public bool TrySave<T>(string fileName, T data) where T : class
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                Debug.LogError("JsonSaveLoadService: fileName is empty.");
                return false;
            }

            if (data == null)
            {
                Debug.LogError("JsonSaveLoadService: data is null.");
                return false;
            }

            var statePath = BuildPath(fileName);
            var backupPath = statePath + BackupSuffix;
            var tempPath = statePath + TempSuffix;

            try
            {
                var directoryPath = Path.GetDirectoryName(statePath);
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var json = JsonUtility.ToJson(data);
                File.WriteAllText(tempPath, json);

                if (File.Exists(statePath))
                {
                    File.Copy(statePath, backupPath, true);
                    File.Delete(statePath);
                }

                File.Move(tempPath, statePath);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save data to {statePath}: {ex}");
                TryDeleteTemp(tempPath);
                return false;
            }
        }

        private string BuildPath(string fileName)
        {
            return Path.Combine(_storageDirectory, fileName);
        }

        private bool TryLoadFromPath<T>(string path, out T data) where T : class
        {
            data = null;
            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                var json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return false;
                }

                data = JsonUtility.FromJson<T>(json);
                return data != null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load data from {path}: {ex}");
                return false;
            }
        }

        private static void TryDeleteTemp(string tempPath)
        {
            try
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to clean temp state file: {ex}");
            }
        }
    }
}
