public interface IItemModifierPriority
{
    /// <summary>
    /// Lower values run earlier; higher values run later. Default priority is 0.
    /// </summary>
    int Priority { get; }
}
