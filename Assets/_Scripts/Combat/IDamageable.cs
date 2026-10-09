public interface IDamageable
{
    /// <summary>
    /// Attempts to apply damage and returns the amount of damage
    /// actually dealt. Returns 0 if damage was not applied.
    /// </summary>
    int TakeDamage(int amount);
}
