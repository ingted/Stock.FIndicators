namespace Skender.Stock.Indicators;

// EXPONENTIAL MOVING AVERAGE (API)
public static partial class Indicator
{
    // SERIES, from TQuote
    /// <include file='./info.xml' path='info/type[@name="standard"]/*' />
    ///
    public static IEnumerable<EmaResult> GetEma<TQuote>(
        this IEnumerable<TQuote> quotes,
        int lookbackPeriods)
        where TQuote : IQuote => quotes
            .ToTuple(CandlePart.Close)
            .CalcEma(lookbackPeriods);


    public static TAWrapper<EmaHelper<double>, EmaResult> GetEmaAuto<TQuote>(
        this IEnumerable<TQuote> quotes,
        TAWrapper<EmaHelper<double>, EmaResult>? taw,
        int lookbackPeriods,
        CandlePart cp = CandlePart.Close,
        bool ifRemoveAtEndOfPreviousResult = false)
        where TQuote : IQuote => quotes
            .ToTuple(cp)
            .CalcEmaAuto(
                taw
                , lookbackPeriods
                , ifRemoveAtEndOfPreviousResult
            );

    // SERIES, from CHAIN
    public static IEnumerable<EmaResult> GetEma(
        this IEnumerable<IReusableResult> results,
        int lookbackPeriods) => results
            .ToTuple()
            .CalcEma(lookbackPeriods)
            .SyncIndex(results, SyncType.Prepend);

    // SERIES, from TUPLE
    public static IEnumerable<EmaResult> GetEma(
        this IEnumerable<(DateTime, double)> priceTuples,
        int lookbackPeriods) => priceTuples
            .ToSortedList()
            .CalcEma(lookbackPeriods);

    public static TAWrapper<EmaHelper<double>, EmaResult> GetEmaAuto(
        this IEnumerable<(DateTime, double)> priceTuples,
        TAWrapper<EmaHelper<double>, EmaResult>? taw,
        int lookbackPeriods,
        bool ifRemoveAtEndOfPreviousResult = false)
    {
        taw ??= new TAWrapper<EmaHelper<double>, EmaResult>();
        var updatedTaw =
            priceTuples
            .ToSortedList()
            .CalcEmaAuto(
                taw
                , lookbackPeriods
                , ifRemoveAtEndOfPreviousResult
            );
        return updatedTaw;
    }

    public static ValueTuple<EmaHelper<double>, List<EmaResult>> GetEmaAuto(
        this IEnumerable<(DateTime, double)> priceTuples,
        EmaHelper<double>? helper,
        List<EmaResult>? results,
        int lookbackPeriods,
        bool ifRemoveAtEndOfPreviousResult = false)
    {
        helper ??= new EmaHelper<double>();
        results ??= new List<EmaResult>();
        var updatedTaw =
            priceTuples
            .ToSortedList()
            .CalcEmaAuto(
                helper
                , results
                , lookbackPeriods
                , ifRemoveAtEndOfPreviousResult
            );
        return updatedTaw;
    }

    // STREAM INITIALIZATION, from TQuote
    /// <include file='./info.xml' path='info/type[@name="stream"]/*' />
    ///
    internal static EmaBase InitEma<TQuote>(
        this IEnumerable<TQuote> quotes,
        int lookbackPeriods)
        where TQuote : IQuote
    {
        // convert quotes
        List<(DateTime, double)> tpList
            = quotes.ToTuple(CandlePart.Close);

        return new EmaBase(tpList, lookbackPeriods);
    }

    // STREAM INITIALIZATION, from CHAIN
    internal static EmaBase InitEma(
        this IEnumerable<IReusableResult> results,
        int lookbackPeriods)
    {
        // convert results
        List<(DateTime, double)> tpList
            = results.ToTuple();

        return new EmaBase(tpList, lookbackPeriods);
    }
}
