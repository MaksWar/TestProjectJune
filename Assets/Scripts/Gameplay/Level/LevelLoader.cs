using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Level
{
    public class LevelLoader : ILevelLoader
    {
        private readonly ILevelFiguresFactory _levelFiguresFactory;

        private FigureComponent _currentFigure;

        public LevelLoader(ILevelFiguresFactory levelFiguresFactory)
        {
            _levelFiguresFactory = levelFiguresFactory;
        }

        public async UniTask<FigureComponent> LoadLevel(FigureType type, string id, CancellationToken cancellationToken = default)
        {
            _currentFigure = await _levelFiguresFactory.CreateFigure(type, id, cancellationToken);

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
