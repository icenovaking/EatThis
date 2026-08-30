namespace EatThis.Api.Application;

public interface IRandomSource
{
    int Next(int maxExclusive);
}

public sealed class SystemRandomSource : IRandomSource
{
    public int Next(int maxExclusive)
    {
        if (maxExclusive <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxExclusive));
        }

        return Random.Shared.Next(maxExclusive);
    }
}
