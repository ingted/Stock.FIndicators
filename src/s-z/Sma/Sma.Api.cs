namespace Skender.Stock.Indicators;

// SIMPLE MOVING AVERAGE (API)
public static partial class Indicator
{
    // SERIES, from TQuote
    /// <include file='./info.xml' path='info/type[@name="Main"]/*' />
    ///
    public static IEnumerable<SmaResult> GetSma<TQuote>(
        this IEnumerable<TQuote> quotes,
        int lookbackPeriods)
        where TQuote : IQuote => quotes
            .ToTuple(CandlePart.Close)
            .CalcSma(lookbackPeriods);

    public static TAWrapper<SmaHelper<double>, SmaResult> GetSmaAuto<TQuote>(
        this IEnumerable<TQuote> quotes,
        TAWrapper<SmaHelper<double>, SmaResult>? taw,
        int lookbackPeriods,
        bool ifRemoveAtEndOfPreviousResult = false,
        CandlePart cp = CandlePart.Close)
        where TQuote : IQuote
    {
        taw ??= new TAWrapper<SmaHelper<double>, SmaResult>();
        return quotes
            .ToTuple(cp)
            .CalcSmaAuto(taw, lookbackPeriods, ifRemoveAtEndOfPreviousResult);

    }
    // SERIES, from CHAIN
    public static IEnumerable<SmaResult> GetSma(
        this IEnumerable<IReusableResult> results,
        int lookbackPeriods) => results
            .ToTuple()
            .CalcSma(lookbackPeriods)
            .SyncIndex(results, SyncType.Prepend);

    //SyncIndex 需要原始 seq ，似乎無法搞 increment
    //public static TAWrapper<SmaHelper, SmaResult> GetSmaAuto(
    //    this IEnumerable<IReusableResult> results,
    //    TAWrapper<SmaHelper, SmaResult>? taw,
    //    int lookbackPeriods,
    //    bool ifRemoveAtEndOfPreviousResult = false)
    //{
    //    taw ??= new TAWrapper<SmaHelper, SmaResult>();
    //    var tawOut = results
    //        .ToTuple()
    //        .CalcSmaAuto(taw, lookbackPeriods, ifRemoveAtEndOfPreviousResult);

    //    tawOut.results.SyncIndex(tawOut.results, SyncType.Prepend);
    //    return taw;
    //}
    // SERIES, from TUPLE
    public static IEnumerable<SmaResult> GetSma(
        this IEnumerable<(DateTime, double)> priceTuples,
        int lookbackPeriods) => priceTuples
            .ToSortedList()
            .CalcSma(lookbackPeriods);

    public static TAWrapper<SmaHelper<double>, SmaResult> GetSmaAuto(
        this IEnumerable<(DateTime, double)> priceTuples,
        TAWrapper<SmaHelper<double>, SmaResult>? taw,
        int lookbackPeriods,
        bool ifRemoveAtEndOfPreviousResult = false)
    {
        taw ??= new TAWrapper<SmaHelper<double>, SmaResult>();
        return
            priceTuples
            .ToSortedList()
            .CalcSmaAuto(taw, lookbackPeriods, ifRemoveAtEndOfPreviousResult);
    }

    public static TAWrapper<SmaHelper<T>, double?> GetSmaScalarAuto<T>(
        this IEnumerable<T> prices,
        TAWrapper<SmaHelper<T>, double?>? taw,
        int lookbackPeriods,
        bool ifRemoveAtEndOfPreviousResult = false)
    {
        taw ??= new TAWrapper<SmaHelper<T>, double?>();
        return
            prices
            .CalcSmaScalarAuto(taw, lookbackPeriods, ifRemoveAtEndOfPreviousResult);
    }


    /// <include file='./info.xml' path='info/type[@name="Analysis"]/*' />
    ///
    // ANALYSIS, from TQuote
    public static IEnumerable<SmaAnalysis> GetSmaAnalysis<TQuote>(
        this IEnumerable<TQuote> quotes,
        int lookbackPeriods)
        where TQuote : IQuote => quotes
            .ToTuple(CandlePart.Close)
            .CalcSmaAnalysis(lookbackPeriods);

    // ANALYSIS, from CHAIN
    public static IEnumerable<SmaAnalysis> GetSmaAnalysis(
        this IEnumerable<IReusableResult> results,
        int lookbackPeriods) => results
            .ToTuple()
            .CalcSmaAnalysis(lookbackPeriods)
            .SyncIndex(results, SyncType.Prepend);

    // ANALYSIS, from TUPLE
    public static IEnumerable<SmaAnalysis> GetSmaAnalysis(
        this IEnumerable<(DateTime, double)> priceTuples,
        int lookbackPeriods) => priceTuples
            .ToSortedList()
            .CalcSmaAnalysis(lookbackPeriods);
}
