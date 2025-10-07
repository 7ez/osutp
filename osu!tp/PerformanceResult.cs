namespace osutp;

public class TpPerformanceResult
{
    public double Acc;
    public double Aim;
    public double Speed;
    public double Total;

    internal TpPerformanceResult()
    {
        Total = 0.0d;
        Aim = 0.0d;
        Speed = 0.0d;
        Acc = 0.0d;
    }
}