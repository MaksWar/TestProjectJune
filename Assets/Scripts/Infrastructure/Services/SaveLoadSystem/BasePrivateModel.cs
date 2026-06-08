using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Infrastructure.Services.SaveLoadSystem
{
    public abstract class BasePrivateModel<T> : IPrivateModel where T : BasePrivateModelScheme, new()
    {
        public virtual string FileName => DataName;
        
        protected T Data;

        private int _frames = 0;
        private bool _saveRequestPending = false;

        public abstract string DataName { get; }

        public bool SaveLock { get; set; }
        public int Tick { get; set; }

        protected bool DontSendChanges { get; private set; }
        protected bool IsReadonly { get; private set; }

        public BasePrivateModel()
        {
            IsReadonly = false;
            SaveLock = false;
            Tick = 0;
        }

        public BasePrivateModel(bool isReadonly, bool dontSendChanges = false)
        {
            IsReadonly = isReadonly;
            DontSendChanges = dontSendChanges;
        }

        public virtual async UniTask Merge(Dictionary<string, object> inData)
        {
            if (Data == null)
            {
                Debug.LogError($"Merge not inited document! {GetType()}");
            }

            Data.Deserialize(inData);
        }

        public virtual Dictionary<string, object> GetChanges()
        {
            if (IsReadonly || DontSendChanges)
            {
                return null;
            }
            
            return Data.GetDirtyData();
        }

        public virtual void ChangesSaved(List<string> properties)
        {
            if (IsReadonly || DontSendChanges)
            {
                return;
            }

            Data.ClearDirtyKeys();
        }

        public virtual void Replace(T data) =>
            Data = data;

        public virtual void Initialize() =>
            Data ??= new T();

        public virtual void Setup() =>
            Data = new T();

        protected void SetKeyDirty(string key, bool notify = true)
        {
            if (DontSendChanges)
            {
                return;
            }

            Data.AddDirtyKey(key);
            if (notify)
            {
                IPrivateModel.OnSave?.Invoke();
            }
        }
    }
}