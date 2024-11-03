namespace Skender.Stock.Indicators;


[Serializable]
public class MacdHelper<T>
{
    public TAWrapper<EmaHelper<T>, EmaResult> ehf = new TAWrapper<EmaHelper<T>, EmaResult>();
    public TAWrapper<EmaHelper<T>, EmaResult> ehs = new TAWrapper<EmaHelper<T>, EmaResult>();
    public TAWrapper<EmaHelper<T>, EmaResult> sig = new TAWrapper<EmaHelper<T>, EmaResult>();
    public int processedValueCount = 0;
    public List<(DateTime, T)> emaDiff = new List<(DateTime, T)>();
    public int totalValueCount = 0;
}

[Serializable]
public class MacdHelper2<T>
{
    public EmaHelper<T> ehf = new EmaHelper<T>();
    public EmaHelper<T> ehs = new EmaHelper<T>();
    public EmaHelper<T> sig = new EmaHelper<T>();
    public int processedValueCount = 0;
    public List<(DateTime, T)> emaDiff = new List<(DateTime, T)>();
    public int totalValueCount = 0;
}


// MOVING AVERAGE CONVERGENCE/DIVERGENCE (MACD) OSCILLATOR (SERIES)
public static partial class Indicator
{
    internal static List<MacdResult> CalcMacd(
        this List<(DateTime, double)> tpList,
        int fastPeriods,
        int slowPeriods,
        int signalPeriods)
    {
        // check parameter arguments
        ValidateMacd(fastPeriods, slowPeriods, signalPeriods);

        // initialize
        List<EmaResult> emaFast = tpList.CalcEma(fastPeriods);
        List<EmaResult> emaSlow = tpList.CalcEma(slowPeriods);

        int length = tpList.Count;
        List<(DateTime, double)> emaDiff = [];
        List<MacdResult> results = new(length);

        // roll through quotes
        for (int i = 0; i < length; i++)
        {
            (DateTime date, double _) = tpList[i];
            EmaResult df = emaFast[i];
            EmaResult ds = emaSlow[i];

            MacdResult r = new(date)
            {
                FastEma = df.Ema,
                SlowEma = ds.Ema
            };
            results.Add(r);

            if (i >= slowPeriods - 1)
            {
                double macd = (df.Ema - ds.Ema).Null2NaN();
                r.Macd = macd.NaN2Null();

                // temp data for interim EMA of macd
                (DateTime, double) diff = (date, macd);

                emaDiff.Add(diff);
            }
        }

        // add signal and histogram to result
        List<EmaResult> emaSignal = CalcEma(emaDiff, signalPeriods);

        for (int d = slowPeriods - 1; d < length; d++)
        {
            MacdResult r = results[d];
            EmaResult ds = emaSignal[d + 1 - slowPeriods];

            r.Signal = ds.Ema.NaN2Null();
            r.Histogram = (r.Macd - r.Signal).NaN2Null();
        }

        return results;
    }


    public static TAWrapper<MacdHelper<double>, MacdResult> CalcMacdAuto(
        this List<(DateTime, double)> tpList,
        TAWrapper<MacdHelper<double>, MacdResult>? taw,
        int fastPeriods,
        int slowPeriods,
        int signalPeriods,
        //CandlePart cp = CandlePart.HL2C4,
        bool ifRemoveAtEndOfPreviousResult = false)
    {
        // check parameter arguments
        ValidateMacd(fastPeriods, slowPeriods, signalPeriods);

        // initialize

        taw ??= new TAWrapper<MacdHelper<double>, MacdResult>();

        var helper = taw.helper;
        var emaFastTaw = tpList.CalcEmaAuto(helper.ehf, fastPeriods, ifRemoveAtEndOfPreviousResult);
        var emaSlowTaw = tpList.CalcEmaAuto(helper.ehs, slowPeriods, ifRemoveAtEndOfPreviousResult);

        int length = tpList.Count;
        var emaDiff = helper.emaDiff;
        var emaDiffTmp = new List<(DateTime, double)>();
        var results = taw.results;


        if (ifRemoveAtEndOfPreviousResult && tpList.Count > 0)
        {
            //if (results.Count - 1 >= slowPeriods)
            //{
            if (results.Count > 0)
            {
                results.RemoveAt(results.Count - 1);
            }
            //}
            if (emaDiff.Count > 0)
            { 
                emaDiff.RemoveAt(emaDiff.Count - 1);
            }
            if (helper.totalValueCount > 0)
            {
                helper.totalValueCount--;
            }
            if (helper.processedValueCount > 0)
            {
                helper.processedValueCount--;
            }
            //20240209: CalcEmaAuto 自己就會 remove
            //helper.ehf.results.RemoveAt(helper.ehf.results.Count - 1);
            //helper.ehs.results.RemoveAt(helper.ehs.results.Count - 1);
            //if (helper.sig.results.Count > 0)
            //{
            //    helper.sig.results.RemoveAt(helper.sig.results.Count - 1);
            //}
        }
        //else
        //{
        //    emaDiff.Clear();
        //}

        var prevTotalCount = helper.totalValueCount;

        if (helper.processedValueCount == 0) //初始化
        {
            helper.totalValueCount = length;
        }
        else
        {
            helper.totalValueCount += length;
        }

        var curProcessedValueCount = helper.processedValueCount;
        // roll through quotes
        for (int i = 0; i + curProcessedValueCount < helper.totalValueCount; i++)
        {
            (DateTime date, double _) = tpList[i];
            EmaResult df = emaFastTaw.results[i + curProcessedValueCount];
            EmaResult ds = emaSlowTaw.results[i + curProcessedValueCount];

            MacdResult r = new(date)
            {
                FastEma = df.Ema,
                SlowEma = ds.Ema
            };
            results.Add(r);

            if (i + curProcessedValueCount >= slowPeriods - 1)
            {
                double macd = (df.Ema - ds.Ema).Null2NaN();
                r.Macd = macd.NaN2Null();
                

                // temp data for interim EMA of macd
                (DateTime, double) diff = (date, macd);

                emaDiffTmp.Clear();
                emaDiffTmp.Add(diff);
                

                if (i + curProcessedValueCount > slowPeriods + signalPeriods - 2)
                {
                    var emaSignalTaw = emaDiffTmp.CalcEmaAuto(taw.helper.sig, signalPeriods, ifRemoveAtEndOfPreviousResult);
                    var er = emaSignalTaw.results[i + curProcessedValueCount - slowPeriods + 1];
                    r.Signal = er.Ema.NaN2Null();
                    r.Histogram = (r.Macd - r.Signal).NaN2Null();
                    //if (ifRemoveAtEndOfPreviousResult)
                    //{
                    //    ifRemoveAtEndOfPreviousResult = false;
                    //}
                }
                else if (i + curProcessedValueCount == slowPeriods + signalPeriods - 2)
                {
                    emaDiff.Add(diff);
                    var emaSignalTaw = emaDiff.CalcEmaAuto(taw.helper.sig, signalPeriods, ifRemoveAtEndOfPreviousResult);
                    var er = emaSignalTaw.results[i + curProcessedValueCount - slowPeriods + 1];
                    r.Signal = er.Ema.NaN2Null();
                    r.Histogram = (r.Macd - r.Signal).NaN2Null();
                }
                else
                {
                    emaDiff.Add(diff);
                }
                    if (ifRemoveAtEndOfPreviousResult)
                    {
                        ifRemoveAtEndOfPreviousResult = false;
                    }
            }
            //else
            //{
            //    (DateTime, double) diff = (date, 0.0);
            //    emaDiff.Add(diff);
            //}
            helper.processedValueCount++;
        }
        return taw;
    }

    public static TAWrapper<MacdHelper2<double>, MacdResult> CalcMacdAuto2(
        this List<(DateTime, double)> tpList,
        TAWrapper<MacdHelper2<double>, MacdResult>? taw,
        int fastPeriods,
        int slowPeriods,
        int signalPeriods,
        //CandlePart cp = CandlePart.HL2C4,
        bool ifRemoveAtEndOfPreviousResult = false)
    {
        // check parameter arguments
        ValidateMacd(fastPeriods, slowPeriods, signalPeriods);

        // initialize

        taw ??= new TAWrapper<MacdHelper2<double>, MacdResult>();

        var helper = taw.helper;
        var emaFastTaw = tpList.CalcEmaAuto(helper.ehf, null, fastPeriods, ifRemoveAtEndOfPreviousResult);
        var emaSlowTaw = tpList.CalcEmaAuto(helper.ehs, null, slowPeriods, ifRemoveAtEndOfPreviousResult);

        int length = tpList.Count;
        var emaDiff = helper.emaDiff;
        var emaDiffTmp = new List<(DateTime, double)>();
        var results = taw.results;


        if (ifRemoveAtEndOfPreviousResult && tpList.Count > 0)
        {
            //if (results.Count - 1 >= slowPeriods)
            //{
            if (results.Count > 0)
            {
                results.RemoveAt(results.Count - 1);
            }
            //}
            if (emaDiff.Count > 0)
            {
                emaDiff.RemoveAt(emaDiff.Count - 1);
            }
            if (helper.totalValueCount > 0)
            {
                helper.totalValueCount--;
            }
            if (helper.processedValueCount > 0)
            {
                helper.processedValueCount--;
            }
            //20240209: CalcEmaAuto 自己就會 remove
            //helper.ehf.results.RemoveAt(helper.ehf.results.Count - 1);
            //helper.ehs.results.RemoveAt(helper.ehs.results.Count - 1);
            //if (helper.sig.results.Count > 0)
            //{
            //    helper.sig.results.RemoveAt(helper.sig.results.Count - 1);
            //}
        }
        //else
        //{
        //    emaDiff.Clear();
        //}

        var prevTotalCount = helper.totalValueCount;

        if (helper.processedValueCount == 0) //初始化
        {
            helper.totalValueCount = length;
        }
        else
        {
            helper.totalValueCount += length;
        }

        var curProcessedValueCount = helper.processedValueCount;
        // roll through quotes
        for (int i = 0; i + curProcessedValueCount < helper.totalValueCount; i++)
        {
            (DateTime date, double _) = tpList[i];
            EmaResult df;
            if (emaFastTaw.Item2.Count > i + curProcessedValueCount)
            {
                df = emaFastTaw.Item2[i + curProcessedValueCount];
            }
            else
            {
                df = emaFastTaw.Item2[^1];
            }
            EmaResult ds;
            if (emaSlowTaw.Item2.Count > i + curProcessedValueCount)
            {
                ds = emaSlowTaw.Item2[i + curProcessedValueCount];
            }
            else
            {
                ds = emaSlowTaw.Item2[^1];
            }
            MacdResult r = new(date)
            {
                FastEma = df.Ema,
                SlowEma = ds.Ema
            };
            results.Add(r);

            if (i + curProcessedValueCount >= slowPeriods - 1)
            {
                double macd = (df.Ema - ds.Ema).Null2NaN();
                r.Macd = macd.NaN2Null();


                // temp data for interim EMA of macd
                (DateTime, double) diff = (date, macd);

                emaDiffTmp.Clear();
                emaDiffTmp.Add(diff);
                

                if (i + curProcessedValueCount > slowPeriods + signalPeriods - 2)
                {
                    var emaSignalTaw = emaDiffTmp.CalcEmaAuto(taw.helper.sig, null, signalPeriods, ifRemoveAtEndOfPreviousResult);
                    var er = emaSignalTaw.Item2[^1];
                    r.Signal = er.Ema.NaN2Null();
                    r.Histogram = (r.Macd - r.Signal).NaN2Null();
                    //if (ifRemoveAtEndOfPreviousResult)
                    //{
                    //    ifRemoveAtEndOfPreviousResult = false;
                    //}
                }
                else if (i + curProcessedValueCount == slowPeriods + signalPeriods - 2)
                {
                    emaDiff.Add(diff);
                    var emaSignalTaw = emaDiff.CalcEmaAuto(taw.helper.sig, null, signalPeriods, ifRemoveAtEndOfPreviousResult);
                    var er = emaSignalTaw.Item2[^1];
                    r.Signal = er.Ema.NaN2Null();
                    r.Histogram = (r.Macd - r.Signal).NaN2Null();
                }
                else
                {
                    emaDiff.Add(diff);
                }
                if (ifRemoveAtEndOfPreviousResult)
                {
                    ifRemoveAtEndOfPreviousResult = false;
                }
            }
            //else
            //{
            //    (DateTime, double) diff = (date, 0.0);
            //    emaDiff.Add(diff);
            //}
            helper.processedValueCount++;
        }
        return taw;
    }

    // parameter validation
    private static void ValidateMacd(
        int fastPeriods,
        int slowPeriods,
        int signalPeriods)
    {
        // check parameter arguments
        if (fastPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fastPeriods), fastPeriods,
                "Fast periods must be greater than 0 for MACD.");
        }

        if (signalPeriods < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(signalPeriods), signalPeriods,
                "Signal periods must be greater than or equal to 0 for MACD.");
        }

        if (slowPeriods <= fastPeriods)
        {
            throw new ArgumentOutOfRangeException(nameof(slowPeriods), slowPeriods,
                "Slow periods must be greater than the fast period for MACD.");
        }
    }
}