namespace Infrastructure.Gameplay
{
    public interface IGameplayContextService
    {
        GameplayLevelPayload LevelPayload { get; }
        bool HasLevelPayload { get; }
        void SetLevelPayload(GameplayLevelPayload payload);
    }
}
