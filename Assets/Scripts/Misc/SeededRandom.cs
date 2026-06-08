using System;

public class SeededRandom
{
    private Random random = new Random();
    private int seed;

    public SeededRandom(int seed)
    {
        this.seed = seed;
        random = new Random(seed);
    }

    public int Next()
    {
        return random.Next();
    }

    public int Next(int maxValue)
    {
        return random.Next(maxValue);
    }

    public int Next(int minValue, int maxValue)
    {
        return random.Next(minValue, maxValue);
    }

    public float Next(float minValue, float maxValue)
    {
        return minValue + (maxValue - minValue) * (float)random.NextDouble();
    }
}