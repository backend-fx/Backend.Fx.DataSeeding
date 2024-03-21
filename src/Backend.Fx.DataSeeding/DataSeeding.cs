namespace Backend.Fx.DataSeeding;

public interface IDataSeeding
{
    DataSeedingLevel Level { get; }
}

public class DataSeeding : IDataSeeding
{
    public DataSeeding(DataSeedingLevel level)
    {
        Level = level;
    }
    
    public DataSeedingLevel Level { get; }
}