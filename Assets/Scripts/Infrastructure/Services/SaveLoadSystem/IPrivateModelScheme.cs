using System.Collections.Generic;

namespace Infrastructure.Services.SaveLoadSystem
{
    public interface IPrivateModelScheme
    {
        void AddDirtyKey(string key);
        void RemoveDirty(List<string> keys);
        Dictionary<string, object> GetDirtyData();
        Dictionary<string, object> Serialize();
        void Deserialize(Dictionary<string, object> data);
    }
}