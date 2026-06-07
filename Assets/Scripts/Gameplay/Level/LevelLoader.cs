using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Level
{
    public class LevelLoader : ILevelLoader
    {
        private readonly ILevelFiguresFactory _levelFiguresFactory;
        private readonly LevelService _levelService;

        private FigureComponent _currentFigure;

        public LevelLoader(ILevelFiguresFactory levelFiguresFactory, LevelService levelService)
        {
            _levelFiguresFactory = levelFiguresFactory;
            _levelService = levelService;
        }

        public async UniTask<FigureComponent> LoadLevel(FigureType type, string id)
        {
            _currentFigure = await _levelFiguresFactory.CreateFigure(type, id);

            await _levelService.Activate(_currentFigure);

            return _currentFigure;
        }

        public void UnLoadCurrentLevel()
        {
            if (_currentFigure == null)
            {
                return;
            }

            Object.Destroy(_currentFigure.gameObject);
            _currentFigure = null;
        }
    }
}
