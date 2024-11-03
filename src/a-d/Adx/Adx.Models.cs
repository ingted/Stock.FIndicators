namespace Skender.Stock.Indicators;

[Serializable]
public sealed class AdxResult : ResultBase, IReusableResult
{
    public enum AdxValTyp
    {
        Pdi = 0,
        Mdi = 1,
        Adx = 2,
        Adxr = 3
    }

    public AdxResult(DateTime date)
    {
        Date = date;
    }

    public double? Pdi { get; set; }
    public double? Mdi { get; set; }
    public double? Adx { get; set; }
    public double? Adxr { get; set; }

    double? IReusableResult.Value => Adx;

    public double? Item(AdxValTyp avt)
    {
        switch (avt)
        {
            case AdxValTyp.Pdi:
                return Pdi;
            case AdxValTyp.Mdi:
                return Mdi;
            case AdxValTyp.Adx:
                return Adx;
            case AdxValTyp.Adxr:
                return Adxr;
            default:
                return null;

        }
    }

    public override string ToString() => $"{Pdi} {Mdi} {Adx} {Adxr}";
}
