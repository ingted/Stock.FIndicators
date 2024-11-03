namespace Skender.Stock.Indicators;

[Serializable]
public class EmaHelper<T>
{
    public int processedValueCount = 0;
    public int totalValueCount = 0; //加入當次
    public int initPeriods;
    public T lastEma;
    //public T preLastEma;
    public EmaHelper<T>? prevHelper = null;

}
// EXPONENTIAL MOVING AVERAGE (SERIES)
public static partial class Indicator
{
    internal static List<EmaResult> CalcEma(
        this List<(DateTime, double)> tpList,
        int lookbackPeriods)
    {
        // check parameter arguments
        EmaBase.Validate(lookbackPeriods);

        // initialize
        int length = tpList.Count;
        List<EmaResult> results = new(length);

        double lastEma = 0;
        double k = 2d / (lookbackPeriods + 1);
        int initPeriods = Math.Min(lookbackPeriods, length);

        for (int i = 0; i < initPeriods; i++)
        {
            (DateTime _, double value) = tpList[i];
            lastEma += value;
        }

        lastEma /= lookbackPeriods;

        // roll through quotes
        for (int i = 0; i < length; i++)
        {
            (DateTime date, double value) = tpList[i];
            EmaResult r = new(date);
            results.Add(r);

            if (i + 1 > lookbackPeriods)
            {
                double ema = EmaBase.Increment(value, lastEma, k);
                r.Ema = ema.NaN2Null();
                lastEma = ema;
            }
            else if (i == lookbackPeriods - 1)
            {
                r.Ema = lastEma.NaN2Null();
            }
        }

        return results;
    }

    public static TAWrapper<EmaHelper<double>, EmaResult> CalcEmaAuto(
        this List<(DateTime, double)> tpList, //首次會是第一個 Ema 值出現的根數，後續每次都是一根
        TAWrapper<EmaHelper<double>, EmaResult>? taw,
        int lookbackPeriods,
        bool ifRemoveAtEndOfPreviousResult = false)
    {
        // check parameter arguments
        EmaBase.Validate(lookbackPeriods);

        // initialize
        int length = tpList.Count;
        taw ??= new TAWrapper<EmaHelper<double>, EmaResult>();
        var results = taw.results;
        var helper = taw.helper;

        var (h, r) = CalcEmaAuto(tpList, helper, results, lookbackPeriods, ifRemoveAtEndOfPreviousResult);

        taw.helper = h;

        return taw;
    }


    public static TAWrapper<EmaHelper<double>, EmaResult> CalcEmaAuto_DEPRECATED(
    this List<(DateTime, double)> tpList, //首次會是第一個 Ema 值出現的根數，後續每次都是一根
    TAWrapper<EmaHelper<double>, EmaResult>? taw,
    int lookbackPeriods,
    bool ifRemoveAtEndOfPreviousResult = false)
    {
        // check parameter arguments
        EmaBase.Validate(lookbackPeriods);

        // initialize
        int length = tpList.Count;
        taw ??= new TAWrapper<EmaHelper<double>, EmaResult>();
        var results = taw.results;
        var helper = taw.helper;

        if (ifRemoveAtEndOfPreviousResult)
        {
            if (results.Count > 0)
            {
                var prevHelper = helper.prevHelper.prevHelper;
                var prePreHelper = prevHelper.prevHelper;
                helper.processedValueCount = prevHelper.processedValueCount;
                helper.totalValueCount = prevHelper.processedValueCount;
                helper.initPeriods = prevHelper.initPeriods;
                helper.lastEma = prevHelper.lastEma;
                helper.prevHelper = prePreHelper;
                results.RemoveAt(results.Count - 1);
            }
        }
        //else
        //{
        //    var prevHelper = new EmaHelper<double>();
        //    prevHelper.processedValueCount = helper.processedValueCount;
        //    prevHelper.totalValueCount = helper.totalValueCount;
        //    prevHelper.initPeriods = helper.initPeriods;
        //    prevHelper.lastEma = helper.lastEma;
        //    helper.prevHelper = prevHelper;
        //}

        var prevTotalCount = helper.totalValueCount;
        var k = 2d / (lookbackPeriods + 1);

        if (helper.processedValueCount == 0) //初始化
        {
            helper.lastEma = 0;
            helper.totalValueCount = length;
        }
        else
        {
            helper.totalValueCount += length;
        }

        helper.initPeriods = Math.Min(lookbackPeriods, helper.totalValueCount);



        //for (int i = prevTotalCount; i < helper.initPeriods; i++)
        //{
        //    (DateTime _, double value) = tpList[i];
        //    helper.lastEma += value;
        //}

        var curProcessedValueCount = helper.processedValueCount;
        // roll through quotes
        for (int i = 0; i + curProcessedValueCount < helper.totalValueCount; i++)
        {
            (DateTime date, double value) = tpList[i];
            EmaResult r = new(date);
            results.Add(r);

            if (i + curProcessedValueCount + 1 > lookbackPeriods)
            {
                double ema = EmaBase.Increment(value, helper.lastEma, k);
                r.Ema = ema.NaN2Null();
                helper.lastEma = ema;
            }
            else if (i + curProcessedValueCount + 1 == lookbackPeriods)
            {
                helper.lastEma += value;
                helper.lastEma /= lookbackPeriods;
                r.Ema = helper.lastEma.NaN2Null();
            }
            else
            {
                //(DateTime _, double value) = tpList[i];
                helper.lastEma += value;
            }


            helper.processedValueCount += 1;
            if (true)
            {
                var prevHelper = new EmaHelper<double>();
                prevHelper.processedValueCount = helper.processedValueCount;
                prevHelper.totalValueCount = helper.totalValueCount;
                prevHelper.initPeriods = helper.initPeriods;
                prevHelper.lastEma = helper.lastEma;
                prevHelper.prevHelper = helper.prevHelper;
                if (prevHelper.prevHelper != null)
                {
                    if (prevHelper.prevHelper.prevHelper != null)
                    {
                        if (prevHelper.prevHelper.prevHelper.prevHelper != null)
                        {
                            prevHelper.prevHelper.prevHelper.prevHelper = null;
                        }
                    }
                }
                helper.prevHelper = prevHelper;
            }
        }

        //helper.processedValueCount += length;

        return taw;
    }

    public static ValueTuple<EmaHelper<double>, List<EmaResult>> CalcEmaAuto(
        this List<(DateTime, double)> tpList, //首次會是第一個 Ema 值出現的根數，後續每次都是一根
        EmaHelper<double>? helper,
        List<EmaResult>? results,
        int lookbackPeriods,
        bool ifRemoveAtEndOfPreviousResult = false)
    {
        // check parameter arguments
        EmaBase.Validate(lookbackPeriods);

        // initialize
        int length = tpList.Count;
        helper ??= new EmaHelper<double>();
        results ??= new List<EmaResult>();
        

        if (ifRemoveAtEndOfPreviousResult)
        {
            //if (helper.processedValueCount > 0)
            //{
            //    var prevHelper = helper.prevHelper.prevHelper;
            //    var prePreHelper = prevHelper.prevHelper;
            //    helper.processedValueCount = prevHelper.processedValueCount;
            //    helper.totalValueCount = prevHelper.processedValueCount;
            //    helper.initPeriods = prevHelper.initPeriods;
            //    helper.lastEma = prevHelper.lastEma;
            //    //helper.preLastEma = prevHelper.preLastEma;
            //    helper.prevHelper = prePreHelper;
                
            //}
            if (helper.processedValueCount > 0)
            {
                var prevHelper = helper.prevHelper;
                var prePreHelper = prevHelper.prevHelper;
                helper.processedValueCount = prevHelper.processedValueCount;
                helper.totalValueCount = prevHelper.processedValueCount;
                helper.initPeriods = prevHelper.initPeriods;
                helper.lastEma = prevHelper.lastEma;
                //helper.preLastEma = prevHelper.preLastEma;
                helper.prevHelper = prePreHelper;
                if (results.Count > 0)
                {
                    results.RemoveAt(results.Count - 1);
                }
            }
        }
        //else
        //{
        //    var prevHelper = new EmaHelper<double>();
        //    prevHelper.processedValueCount = helper.processedValueCount;
        //    prevHelper.totalValueCount = helper.totalValueCount;
        //    prevHelper.initPeriods = helper.initPeriods;
        //    prevHelper.lastEma = helper.lastEma;
        //    helper.prevHelper = prevHelper;
        //}

        var prevTotalCount = helper.totalValueCount;
        var k = 2d / (lookbackPeriods + 1);

        if (helper.processedValueCount == 0) //初始化
        {
            helper.lastEma = 0;
            helper.totalValueCount = length;
        }
        else
        {
            helper.totalValueCount += length;
        }

        helper.initPeriods = Math.Min(lookbackPeriods, helper.totalValueCount);



        //for (int i = prevTotalCount; i < helper.initPeriods; i++)
        //{
        //    (DateTime _, double value) = tpList[i];
        //    helper.lastEma += value;
        //}

        var curProcessedValueCount = helper.processedValueCount;
        // roll through quotes
        for (int i = 0; i + curProcessedValueCount < helper.totalValueCount; i++)
        {
            //if (false)
            //{
                var prevHelper = new EmaHelper<double>();
                prevHelper.processedValueCount = helper.processedValueCount;
                prevHelper.totalValueCount = helper.totalValueCount;
                prevHelper.initPeriods = helper.initPeriods;
                prevHelper.lastEma = helper.lastEma;
                //prevHelper.preLastEma = helper.preLastEma;
                helper.prevHelper = prevHelper;
                
            //}
            (DateTime date, double value) = tpList[i];
            EmaResult r = new(date);
            results.Add(r);
            //helper.preLastEma = helper.lastEma;
            if (i + curProcessedValueCount + 1 > lookbackPeriods)
            {
                double ema = EmaBase.Increment(value, helper.lastEma, k);
                r.Ema = ema.NaN2Null();
                helper.lastEma = ema;
            }
            else if (i + curProcessedValueCount + 1 == lookbackPeriods)
            {
                helper.lastEma += value;
                helper.lastEma /= lookbackPeriods;
                r.Ema = helper.lastEma.NaN2Null();
            }
            else
            {
                //(DateTime _, double value) = tpList[i];
                helper.lastEma += value;
            }


            helper.processedValueCount += 1;
            
        }

        //helper.processedValueCount += length;

        return (helper, results);
    }
}
