namespace Skender.Stock.Indicators;

public static class DoubleHelper
{
    public static string Str(this double value)
    {
        return String.Format("{0:000.00}", value);
    }
}
public class AdxHelper : ICloneable
{
    public double prevHigh = 0.0;
    public double prevLow = 0.0;
    public double prevClose = 0.0;
    public double prevTrs = 0.0; // smoothed
    public double prevPdm = 0.0;
    public double prevMdm = 0.0;
    public double prevAdx = 0.0;

    public double sumTr = 0.0;
    public double sumPdm = 0.0;
    public double sumMdm = 0.0;
    public double sumDx = 0.0;
    public AdxHelper? prevHelper = null;

    public override string ToString()
    {
        return prevHigh.Str() +   " " +
            prevLow.Str() +       " " +
            prevClose.Str() +     " " +
            prevTrs.Str() +       " " +
            prevPdm.Str() +       " " +
            prevMdm.Str() +       " " +
            prevAdx.Str() +       " " +
            sumTr.Str() +         " " +
            sumPdm.Str() +        " " +
            sumDx.Str();
            

    }

    //1>G:\coldfar_py\sharftrade7\Libs\Stock.Indicators\src\a-d\Adx\Adx.Series.cs(4,26,4,36): error CS0738: 'AdxHelper' does not implement interface member 'ICloneable.Clone()'. 'AdxHelper.Clone()' cannot implement 'ICloneable.Clone()' because it does not have the matching return type of 'object'.
    public object Clone()
    {
        //var ph = prevHelper;
        //prevHelper = null;
        //var rtn = MemberwiseClone();
        //prevHelper = ph;
        //return (object)(MemberwiseClone(), ph);
        return (object)MemberwiseClone();
    }

    public AdxHelper Dup()
    {
        //var cloned = (ValueTuple<object, AdxHelper>)Clone();
        //prevHelper = cloned.Item2;
        return (AdxHelper)Clone();
    }
}
// AVERAGE DIRECTIONAL INDEX (SERIES)

public class AdxWrapper
{
    public AdxWrapper(AdxHelper helper, List<AdxResult> results)
    {
        this.results = results;
        this.helper = helper;
    }

    public AdxWrapper()
    {
        this.results = new List<AdxResult>();
        this.helper = new AdxHelper();
    }

    public AdxHelper helper;
    public List<AdxResult> results;
    public override string ToString()
    {
        var cnt = results.Count;
        var rst0 = cnt > 0 ? $" {results[0]}" : String.Empty;
        var rstLast = cnt > 0 ? $"-{results[^1]}, " : String.Empty;
        return $"count: {cnt},{rst0}{rstLast} helperPrevAdx: {helper.prevAdx}, prevHelper_is_null: {helper.prevHelper == null}";
    }
}

public static partial class Indicator
{
    internal static Tuple<List<AdxResult>, AdxHelper> CalcAdx(
        this List<QuoteD> qdList,
        int lookbackPeriods,
        AdxHelper ah,
        List<AdxResult> results,
        int start = 0
        )
    {
        return CalcAdx(
            qdList,
            lookbackPeriods,
            lookbackPeriods,
            ah,
            results,
            start);
    }
    internal static Tuple<List<AdxResult>, AdxHelper> CalcAdx(
        this List<QuoteD> qdList,
        int lookbackPeriods,
        int lookbackPeriods2,
        AdxHelper? ah,
        List<AdxResult>? results,
        int start = 0
        )
    {
        // check parameter arguments
        ValidateAdx(lookbackPeriods);

        // initialize
        int length = qdList.Count;
        //if (results == null)
        //{
        //    results = new(length);
        //}
        results ??= new();
        var skippedResultCount = results.Count;

        //double prevHigh = 0;
        //double prevLow = 0;
        //double prevClose = 0;
        //double prevTrs = 0; // smoothed
        //double prevPdm = 0;
        //double prevMdm = 0;
        //double prevAdx = 0;

        //double sumTr = 0;
        //double sumPdm = 0;
        //double sumMdm = 0;
        //double sumDx = 0;

        //代表前一次的 AdxHelper
        AdxHelper? prevAH = null;
        ;
        // Console.WriteLine($"start: {start}");
        if (ah == null)
        {
#if DEBUG2
            Console.WriteLine("ah is null!!!");
#endif
            ah = new AdxHelper();
            start = 0;
        }
        var ifInit = true;

        if (ah.prevHelper != null)
        {
            //ah = ah.prevHelper;
            ifInit = false;
            // ah.prevHelper = ah;
        }


        // roll through quotes
        for (int i = start; i < length; i++)
        {
#if DEBUG2
            Console.WriteLine(ah.ToString() + " => begin");
#endif
            // Console.WriteLine(i);
            if (!ifInit)
            {
                var ph = ah.prevHelper?.Dup();
                //20240326 因為序列化太久，所以加入此段，清除不必要的 field chain
                //if (ah.prevHelper != null)
                //{
                //    if (ph.prevHelper != null)
                //    {
                //        ph.prevHelper = null;
                //    }
                //}

                prevAH = ah.Dup();
                prevAH.prevHelper = ph;
                ah.prevHelper = prevAH;
                //ifInit = false;
            }
            QuoteD q = qdList[i];

            AdxResult r = new(q.Date);
            results.Add(r);

            // skip first period
            if (i == 0 && ifInit)
            {
                ah.prevHigh = q.High;
                ah.prevLow = q.Low;
                ah.prevClose = q.Close;
                ifInit = false;
#if DEBUG2
                Console.WriteLine(ah.ToString());
#endif
                continue;
            }

            double hmpc = Math.Abs(q.High - ah.prevClose);
            double lmpc = Math.Abs(q.Low - ah.prevClose);
            double hmph = q.High - ah.prevHigh;
            double plml = ah.prevLow - q.Low;

            double tr = Math.Max(q.High - q.Low, Math.Max(hmpc, lmpc));

            double pdm1 = hmph > plml ? Math.Max(hmph, 0) : 0;
            double mdm1 = plml > hmph ? Math.Max(plml, 0) : 0;

            ah.prevHigh = q.High;
            ah.prevLow = q.Low;
            ah.prevClose = q.Close;

            // initialization period

            var fixedI = i + skippedResultCount;

            if (fixedI <= lookbackPeriods)
            {
                ah.sumTr += tr;
                ah.sumPdm += pdm1;
                ah.sumMdm += mdm1;
            }

            // skip DM initialization period
            if (fixedI < lookbackPeriods)
            {
#if DEBUG2
                Console.WriteLine(ah.ToString());
#endif
                continue;
            }

            // smoothed true range and directional movement
            double trs;
            double pdm;
            double mdm;

            if (fixedI == lookbackPeriods)
            {
                trs = ah.sumTr;
                pdm = ah.sumPdm;
                mdm = ah.sumMdm;
            }
            else
            {
                trs = ah.prevTrs - (ah.prevTrs / lookbackPeriods) + tr;
                pdm = ah.prevPdm - (ah.prevPdm / lookbackPeriods) + pdm1;
                mdm = ah.prevMdm - (ah.prevMdm / lookbackPeriods) + mdm1;
            }

            ah.prevTrs = trs;
            ah.prevPdm = pdm;
            ah.prevMdm = mdm;

            if (trs is 0)
            {
                continue;
            }

            // directional increments
            double pdi = 100 * pdm / trs;
            double mdi = 100 * mdm / trs;

            r.Pdi = pdi;
            r.Mdi = mdi;

            // calculate ADX
            double dx = (pdi == mdi)
                ? 0
                : (pdi + mdi != 0)
                ? 100 * Math.Abs(pdi - mdi) / (pdi + mdi)
                : double.NaN;

            double adx;

            if (fixedI > (2 * lookbackPeriods) - 1)
            {
                adx = ((ah.prevAdx * (lookbackPeriods - 1)) + dx) / lookbackPeriods;
                r.Adx = adx.NaN2Null();

                double? priorAdx = results[fixedI - lookbackPeriods2].Adx;

                r.Adxr = (adx + priorAdx).NaN2Null() / 2;
                ah.prevAdx = adx;
            }

            // initial ADX
            else if (fixedI == (2 * lookbackPeriods) - 1)
            {
                ah.sumDx += dx;
                adx = ah.sumDx / lookbackPeriods;
                r.Adx = adx.NaN2Null();
                ah.prevAdx = adx;
            }
            else if (fixedI == (2 * lookbackPeriods) - 2)
            {
                ah.sumDx += dx;
                r.Adx = (ah.sumDx / (lookbackPeriods - 1)).NaN2Null();
            }

            // ADX initialization period
            else
            {
                ah.sumDx += dx;
            }
#if DEBUG2
            Console.WriteLine(ah.ToString());
#endif
        }

        return Tuple.Create(results, ah);
    }

    // parameter validation
    private static void ValidateAdx(
        int lookbackPeriods)
    {
        // check parameter arguments
        if (lookbackPeriods <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 1 for ADX.");
        }
    }
}
