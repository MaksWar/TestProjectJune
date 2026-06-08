using Infrastructure.AssetManagement;
using Utilities.Pool;
using Zenject;

namespace Gameplay.Level
{
    public class FigurePointerPool : ObjectPool<FigurePointerComponent>
    {
        public FigurePointerPool(IAssetsProvider assetsProvider, DiContainer container)
            : base(assetsProvider, container)
        {
        }
    }
}