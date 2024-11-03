namespace Skender.Stock.Indicators;

// MOVING AVERAGE CONVERGENCE/DIVERGENCE (MACD) OSCILLATOR (API)
public static partial class Indicator
{
    // SERIES, from TQuote
    /// <include file='./info.xml' path='info/*' />
    ///
    public static IEnumerable<MacdResult> GetMacd<TQuote>(
        this IEnumerable<TQuote> quotes,
        int fastPeriods = 12,
        int slowPeriods = 26,
        int signalPeriods = 9)
        where TQuote : IQuote => quotes
            .ToTuple(CandlePart.Close)
            .CalcMacd(fastPeriods, slowPeriods, signalPeriods);


    public static TAWrapper<MacdHelper<double>, MacdResult> GetMacdAuto<TQuote>(
        this IEnumerable<TQuote> quotes,
        TAWrapper<MacdHelper<double>, MacdResult>? taw,
        int fastPeriods = 12,
        int slowPeriods = 26,
        int signalPeriods = 9,
        CandlePart cp = CandlePart.HL2C4,
        bool ifRemoveAtEndOfPreviousResult = false)
        where TQuote : IQuote
    {
        taw ??= new TAWrapper<MacdHelper<double>, MacdResult>();
        return
            quotes
            .ToTuple(cp)
            .CalcMacdAuto(taw, fastPeriods, slowPeriods, signalPeriods, ifRemoveAtEndOfPreviousResult);
    }
    public static TAWrapper<MacdHelper2<double>, MacdResult> GetMacdAuto2<TQuote>(
       this IEnumerable<TQuote> quotes,
       TAWrapper<MacdHelper2<double>, MacdResult>? taw,
       int fastPeriods = 12,
       int slowPeriods = 26,
       int signalPeriods = 9,
       CandlePart cp = CandlePart.HL2C4,
       bool ifRemoveAtEndOfPreviousResult = false)
       where TQuote : IQuote
    {
        taw ??= new TAWrapper<MacdHelper2<double>, MacdResult>();
        return
            quotes
            .ToTuple(cp)
            .CalcMacdAuto2(taw, fastPeriods, slowPeriods, signalPeriods, ifRemoveAtEndOfPreviousResult);
    }
    // SERIES, from CHAIN
    public static IEnumerable<MacdResult> GetMacd(
        this IEnumerable<IReusableResult> results,
        int fastPeriods = 12,
        int slowPeriods = 26,
        int signalPeriods = 9) => results
            .ToTuple()
            .CalcMacd(fastPeriods, slowPeriods, signalPeriods)
            .SyncIndex(results, SyncType.Prepend);

    // SERIES, from TUPLE
    public static IEnumerable<MacdResult> GetMacd(
        this IEnumerable<(DateTime, double)> priceTuples,
        int fastPeriods = 12,
        int slowPeriods = 26,
        int signalPeriods = 9) => priceTuples
            .ToSortedList()
            .CalcMacd(fastPeriods, slowPeriods, signalPeriods);

    public static TAWrapper<MacdHelper<double>, MacdResult> GetMacdAuto(
        this IEnumerable<(DateTime, double)> priceTuples,
        TAWrapper<MacdHelper<double>, MacdResult>? taw,
        int fastPeriods = 12,
        int slowPeriods = 26,
        int signalPeriods = 9,
        //CandlePart cp = CandlePart.HL2C4,
        bool ifRemoveAtEndOfPreviousResult = false)
    {

        taw ??= new TAWrapper<MacdHelper<double>, MacdResult>();
        return priceTuples
            .ToSortedList()
            .CalcMacdAuto(taw, fastPeriods, slowPeriods, signalPeriods, ifRemoveAtEndOfPreviousResult);
    }

}
