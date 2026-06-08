using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Infrastructure.Services.SaveLoadSystem
{
    public interface IPrivateModelProvider
    {
        /// <summary>
        /// Получение всех моделей по интерфейсу, вызов процесса чтения/обновления в них.
        /// </summary>
        /// <returns></returns>
        UniTask InitializeAsync();

        /// <summary>
        /// Синхронизируем данные, которые пришли с бэкенда с приватными моделями
        /// </summary>
        /// <param name="data">Данные с бэкенда, словарь данных моделей</param>
        UniTask MergeDataFromPull(Dictionary<string, object> data);

        Dictionary<string, Dictionary<string, object>> GetChangedDataToPush(List<string> filter = null);
        T Get<T>() where T : class, IPrivateModel;
        IPrivateModel Get(string dataName);
        List<T> GetAll<T>() where T : class;

        /// <summary>
        /// Добавляет или заменяет имеющуюся модель новой.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        void Set<T>(T item) where T : class, IPrivateModel;
        void ChangesSaved(Dictionary<string, Dictionary<string, object>> data);
    }
}