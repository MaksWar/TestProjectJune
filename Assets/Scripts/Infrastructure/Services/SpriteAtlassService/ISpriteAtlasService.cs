using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Infrastructure.Services.SpriteAtlassService
{
    public interface ISpriteAtlasService : IDisposable
    {
        /// <summary>
        /// Узнает сначала все адреса по лейблу SpriteAtlas.
        /// Потом все их затягивает в память.
        /// </summary>
        /// <returns></returns>
        UniTask InitializeAsync();
        
        /// <summary>
        /// Вытаскивает спрайт по его имени из атласов конкретных типов.
        /// Доступные типы смотры в <see cref="SpriteAtlasManager._atlasesTypes"/>
        /// </summary>
        /// <param name="name">Имя файла</param>
        /// <param name="type">Тип атласа</param>
        /// <returns></returns>
        Sprite GetSprite(string name, string type);
        Sprite GetSprite(string name, List<string> atlasesToSearchLabels);
        bool TryGetSprite(string name, string type, out Sprite sprite);
        Sprite GetDefaultNoneSprite();
        bool IsNoneSprite(Sprite noneSprite);
    }
}