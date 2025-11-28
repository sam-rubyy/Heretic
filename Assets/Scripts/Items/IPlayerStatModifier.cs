public interface IPlayerStatModifier
{
    void Apply(PlayerStats stats);
    void Remove(PlayerStats stats);
}
