using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Infrastructure.Services.SaveLoadSystem
{
    public interface IPrivateModel
    {
        static Action OnSave;
        
        /// <summary>
        /// Под этим именем сохраняем документ на в базу данных
        /// </summary>
        string DataName { get; }

        /// <summary>
        /// Инициализировать документ
        /// </summary>
        void Initialize();

        /// <summary>
        /// Запоминаем какие свойства отправились на сервер
        /// </summary>
        /// <param name="properties">Ключи свойств</param>
        void ChangesSaved(List<string> properties);

        /// <summary>
        /// Делает первоначальную инициализацию модели, наполняет ее дефолтными данными.
        /// </summary>
        void Setup();

        /// <summary>
        /// Обновить свойства в документе на новые
        /// </summary>
        /// <param name="inData">Новые свойства</param>
        UniTask Merge(Dictionary<string, object> inData);

        /// <summary>
        /// Получить свойства которые изменились
        /// </summary>
        /// <returns>Данные которые изменились</returns>
        Dictionary<string, object> GetChanges();
    }
}