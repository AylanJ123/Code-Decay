namespace com.AylanJ123.CodeDecay
{
    /// <summary> Interface for objects that possess health </summary>
    public interface IHealthStats
    {
        void Heal(float amount);
        void Damage(float amount);
    }

    /// <summary> Interface for objects that possess modifiable damage </summary>
    public interface ICombatStats
    {
        void ApplyDamageModification(float amount);
        void RemoveDamageModification(float amount);
    }

    /// <summary> Interface for objects that possess modifiable speed </summary>
    public interface ISpeedStats
    {
        void ApplySpeedModification(float amount);
        void RemoveSpeedModification(float amount);
    }

    /// <summary> Interface for objects that possess modifiable shoot cooldown </summary>
    public interface ICooldownStats
    {
        void ApplyCooldownModification(float amount);
        void RemoveCooldownModification(float amount);
    }

    public interface IStats
    {
        void Cleanse();
    }
}
