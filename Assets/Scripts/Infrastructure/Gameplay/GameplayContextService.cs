namespace Infrastructure.Gameplay
{
    public class GameplayContextService : IGameplayContextService
    {
        public GameplayLevelPayload LevelPayload { get; private set; }
        public bool HasLevelPayload { get; private set; }

        public void SetLevelPayload(GameplayLevelPayload payload)
        {
            LevelPayload = payload;
            HasLevelPayload = true;
        }
    }
}
