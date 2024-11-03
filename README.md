This a cloned and modified repo of DaveSkender/Stock.Indicators

Changes are some util method and classes like the code below and BETTER incremental calculation for ADX/DI/MACD/SNA, yes, BETTER!!


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


[![image](https://raw.githubusercontent.com/DaveSkender/Stock.Indicators/main/docs/assets/social-banner.png)](https://dotnet.StockIndicators.dev/)

[![GitHub Stars](https://img.shields.io/github/stars/DaveSkender/Stock.Indicators?logo=github&label=Stars)](https://github.com/DaveSkender/Stock.Indicators)
[![NuGet package](https://img.shields.io/nuget/v/skender.stock.indicators?color=blue&logo=NuGet&label=NuGet)](https://www.nuget.org/packages/Skender.Stock.Indicators)
[![NuGet](https://img.shields.io/nuget/dt/skender.stock.indicators?logo=NuGet&label=Downloads)](https://www.nuget.org/packages/Skender.Stock.Indicators)
[![code coverage](https://img.shields.io/azure-devops/coverage/skender/stock.indicators/21/main?logo=AzureDevOps&label=Test%20Coverage)](https://dev.azure.com/skender/Stock.Indicators/_build/latest?definitionId=21&branchName=main&view=codecoverage-tab)

# Stock Indicators for .NET

**Stock Indicators for .NET** is a C# [library package](https://www.nuget.org/packages/Skender.Stock.Indicators) that produces financial market technical indicators.  Send in historical price quotes and get back desired indicators such as moving averages, Relative Strength Index, Stochastic Oscillator, Parabolic SAR, etc.  Nothing more.

Build your private technical analysis, trading algorithms, machine learning, charting, or other intelligent market software with this library and your own [OHLCV](https://dotnet.stockindicators.dev/guide/#historical-quotes) price quotes sources for equities, commodities, forex, cryptocurrencies, and others.  [Stock Indicators for Python](https://python.stockindicators.dev/) is also available.

Visit our project site for more information:

- [Overview](https://dotnet.stockindicators.dev/)
- [Indicators and overlays](https://dotnet.stockindicators.dev/indicators/)
- [Guide and Pro tips](https://dotnet.stockindicators.dev/guide/)
- [Demo site](https://charts.stockindicators.dev/) (a stock chart)
- [Release notes](https://github.com/DaveSkender/Stock.Indicators/releases)
- [Discussions](https://github.com/DaveSkender/Stock.Indicators/discussions)
- [Contributing](https://dotnet.stockindicators.dev/contributing/)
