using System.Collections.Generic;

namespace Skender.Stock.Indicators;

// AVERAGE DIRECTIONAL INDEX (API)
public static partial class Indicator
{

    public static Tuple<List<AdxResult>, AdxHelper> GetAdx<TQuote>(
        this IEnumerable<TQuote> quotes,
        AdxHelper ah,
        List<AdxResult> results,
        int start,
        int lookbackPeriods = 7,
        int lookbackPeriods2 = 14
        )
        where TQuote : IQuote
    {
        return quotes
            .ToQuoteD()
            .CalcAdx(lookbackPeriods, lookbackPeriods2, ah, results, start);
    }

    public static AdxWrapper GetAdxAuto(
        this IEnumerable<IQuote> quotes,
        AdxWrapper aw,
        int lookbackPeriods = 7,
        int lookbackPeriods2 = 14
        )
    {
        return GetAdxAuto(
            quotes,
            aw,
            aw.results.Count,
            lookbackPeriods,
            lookbackPeriods2
        );
    }

    public static AdxWrapper GetAdxAuto(
        this IEnumerable<IQuote> quotes,
        AdxWrapper aw,
        int start,
        int lookbackPeriods = 7,
        int lookbackPeriods2 = 14,
        bool ifRemoveAtEndOfPreviousResult = false
        )
    {
        List<AdxResult> results = aw.results;
        // Console.WriteLine($"results.Count: {results.Count}");
        var rCnt = results.Count;
        if (rCnt > 0 && ifRemoveAtEndOfPreviousResult)
        {
            results.RemoveAt(rCnt - 1);
            aw.helper = aw.helper.prevHelper;
        }
        AdxHelper ah = aw.helper;
        var tpl = GetAdx(
            quotes
            , ah
            , results
            , start //> 0 ? results.Count - 1 : 0
            , lookbackPeriods
            , lookbackPeriods2
            );
        aw.helper = tpl.Item2;

        return aw;
    }
}