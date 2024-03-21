namespace Backend.Fx.DataSeeding;

public enum DataSeedingLevel
{
    /// <summary>
    /// Seed plenty of production like data so that every feature can be demonstrated thoroughly (slower)
    /// </summary>
    Demonstration = 1,
    
    /// <summary>
    /// Seed a minimum of production like data so that every feature is functional (faster)
    /// </summary>
    Development = 2,
    
    /// <summary>
    /// Seed only, what is needed for an "empty" database to work
    /// </summary>
    Production = int.MaxValue
}