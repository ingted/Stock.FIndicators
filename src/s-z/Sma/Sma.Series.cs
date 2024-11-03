using System.Linq;

namespace Skender.Stock.Indicators;


[Serializable]
public class TAWrapper<TAHelper, TAResult>
        where TAHelper : new()
    {
        public TAWrapper(TAHelper helper, List<TAResult> results)
        {
            this.results = results;
            this.helper = helper;
        }

        public TAWrapper()
        {
            this.results = new List<TAResult>();
            this.helper = new TAHelper();
        }

        public TAHelper helper;
        public List<TAResult> results;

    //public override string ToString()
    //{
    //    var cnt = results.Count;
    //    var rst0 = cnt > 0 ? $" {results[0]}" : String.Empty;
    //    var rstLast = cnt > 0 ? $"-{results[^1]}, " : String.Empty;
    //    return $"count: {cnt},{rst0}{rstLast} helperPrevAdx: {helper.prevAdx}, prevHelper_is_null: {helper.prevHelper == null}";
    //}
        
}
// SIMPLE MOVING AVERAGE (SERIES)

[Serializable]
public class SmaHelper<T>
{

    //public SmaHelper? prevHelper = null;
    public double curSum = 0;
    public Queue<T> curSumWindow = new Queue<T>();
    public T lastDequeued;
    public double curSumWithoutPrevious = 0;
    public DateTime? curLastDT = null;
    public int curResultIdx = 0;
}


public static partial class Indicator
{



    internal static List<SmaResult> CalcSma(
        this List<(DateTime, double)> tpList,
        int lookbackPeriods)
    {
        // check parameter arguments
        ValidateSma(lookbackPeriods);

        // initialize
        List<SmaResult> results = new(tpList.Count);

        // roll through quotes
        for (int i = 0; i < tpList.Count; i++)
        {
            (DateTime date, double _) = tpList[i];

            SmaResult result = new(date);
            results.Add(result);

            if (i + 1 >= lookbackPeriods)
            {
                double sumSma = 0;
                for (int p = i + 1 - lookbackPeriods; p <= i; p++)
                {
                    (DateTime _, double pValue) = tpList[p];
                    sumSma += pValue;
                }

                result.Sma = (sumSma / lookbackPeriods).NaN2Null();
            }
        }

        return results;
    }



    public static TAWrapper<SmaHelper<double>, SmaResult> CalcSmaAuto(
        this List<(DateTime, double)> tpList,
        TAWrapper<SmaHelper<double>, SmaResult> taw,
        int lookbackPeriods,
        bool ifRemoveAtEndOfPreviousResult = false)
    {
        // check parameter arguments
        ValidateSma(lookbackPeriods);

        // initialize
        List<SmaResult> results = taw.results;
        var rCnt = results.Count;
        var curRCnt = rCnt;
        if (rCnt > 0 && ifRemoveAtEndOfPreviousResult)
        {
            results.RemoveAt(rCnt - 1);
            taw.helper.curSum = taw.helper.curSumWithoutPrevious; //扣掉未完成大 K
            var l = taw.helper.curSumWindow.ToList().Take(taw.helper.curSumWindow.Count - 1);
            taw.helper.curSumWindow = new Queue<double>(l.Prepend(taw.helper.lastDequeued));
            rCnt--;
        }
        // roll through quotes
        bool ifInitThisLoop = true; //表示是否是此次 loop 開始，可能 Sma 已經經過計算多次
        //bool ifFirstResult = rCnt == 0; //表示是否是 Sma 開始計算
        //double newCurSum = 0;
        for (int i = 0; i < tpList.Count; i++)
        {

            //if (rCnt > 30794)
            //{
            //    Console.ReadLine();
            //}
            (DateTime date, double curValue) = tpList[i];

            taw.helper.curSumWindow.Enqueue(curValue);

            SmaResult result = new(date);
            results.Add(result);

            if (i + 1 + rCnt >= lookbackPeriods)
            {
                double sumSma =
                    ifInitThisLoop && ifRemoveAtEndOfPreviousResult ?
                    taw.helper.curSumWithoutPrevious : taw.helper.curSum; //其實此時是 previous sum

                if (curRCnt == 0)
                {
                    for (int p = i + 1 - lookbackPeriods; p <= i; p++)
                    {
                        (DateTime _, double pValue) = tpList[p];
                        sumSma += pValue;
                        taw.helper.curSum = sumSma;
                    }
                }
                else
                {
                    taw.helper.lastDequeued = taw.helper.curSumWindow.Dequeue();
                    taw.helper.curSumWithoutPrevious = taw.helper.curSum;
                    sumSma += curValue - taw.helper.lastDequeued;
                    taw.helper.curSum = sumSma;
                }
                curRCnt++;
                result.Sma = (sumSma / lookbackPeriods).NaN2Null();
                //ifFirstResult = false;
            }
            ifInitThisLoop = false;
        }

        return taw;
    }


    public static TAWrapper<SmaHelper<T>, Nullable<double>> CalcSmaScalarAuto<T>(
        this IEnumerable<T> _tpList,
        TAWrapper<SmaHelper<T>, Nullable<double>> taw,
        int lookbackPeriods,
        bool ifRemoveAtEndOfPreviousResult = false)
    {
        // check parameter arguments
        ValidateSma(lookbackPeriods);
        var tpList = _tpList as List<T>;
        if (tpList == null)
        {
            tpList = _tpList.ToList();
        }
        // initialize
        List<Nullable<double>> results = taw.results;
        var rCnt = results.Count;
        var curRCnt = rCnt;
        if (rCnt > 0 && ifRemoveAtEndOfPreviousResult)
        {
            results.RemoveAt(rCnt - 1);
            taw.helper.curSum = taw.helper.curSumWithoutPrevious; //扣掉未完成大 K
            var l = taw.helper.curSumWindow.ToList().Take(taw.helper.curSumWindow.Count - 1);
            taw.helper.curSumWindow = new Queue<T>(l.Prepend(taw.helper.lastDequeued));
            rCnt--;
        }
        // roll through quotes
        bool ifInitThisLoop = true; //表示是否是此次 loop 開始，可能 Sma 已經經過計算多次
        //bool ifFirstResult = rCnt == 0; //表示是否是 Sma 開始計算
        //double newCurSum = 0;
        for (int i = 0; i < tpList.Count; i++)
        {
            T curValue = tpList[i];

            taw.helper.curSumWindow.Enqueue(curValue);

            Nullable<double> result = null;
            

            if (i + 1 + rCnt >= lookbackPeriods)
            {
                double sumSma =
                    ifInitThisLoop && ifRemoveAtEndOfPreviousResult ?
                    taw.helper.curSumWithoutPrevious : taw.helper.curSum; //其實此時是 previous sum

                if (curRCnt == 0)
                {
                    for (int p = i + 1 - lookbackPeriods; p <= i; p++)
                    {
                        T pValue = tpList[p];
                        sumSma += Convert.ToDouble(pValue);
                        taw.helper.curSum = sumSma;
                    }
                }
                else
                {
                    taw.helper.lastDequeued = taw.helper.curSumWindow.Dequeue();
                    taw.helper.curSumWithoutPrevious = taw.helper.curSum;
                    sumSma += Convert.ToDouble(curValue) - Convert.ToDouble(taw.helper.lastDequeued);
                    taw.helper.curSum = sumSma;
                }
                curRCnt++;
                result = (sumSma / lookbackPeriods).NaN2Null();
                //ifFirstResult = false;
            }
            results.Add(result);
            ifInitThisLoop = false;
        }

        return taw;
    }
    

    // parameter validation
    private static void ValidateSma(
        int lookbackPeriods)
    {
        // check parameter arguments
        if (lookbackPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 0 for SMA.");
        }
    }
}
