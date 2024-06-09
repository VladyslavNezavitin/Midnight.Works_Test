using System;

[Serializable]
public struct CarStats
{
    public float enginePower;
    public float maxSpeed;
    public float control;

    public static CarStats operator +(CarStats stats1, CarStats stats2)
    {
        return new CarStats()
        {
            enginePower = stats1.enginePower + stats2.enginePower,
            maxSpeed = stats1.maxSpeed + stats2.maxSpeed,
            control = stats1.control + stats2.control
        };
    }
}