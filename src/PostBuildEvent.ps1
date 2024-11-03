param(
    $assembly
)
write-host ($assembly + ': Current path: ' + (pwd).path)
cd .\nupkgs\
try {
    $pkg = (dir "$($assembly)*.nupkg" | Sort-Object -Property Name -Descending)[0].Name
    invoke-expression "dotnet nuget push $pkg --api-key $(gc G:\Nuget\apikey.txt) --source https://api.nuget.org/v3/index.json  --skip-duplicate"
}
catch {
    write-host "=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+="
    write-host $_
    write-host "=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+="
}

#dotnet nuget push "G:\coldfar_py\sharftrade7\Libs\Stock.Indicators\src\nupkgs\FAkka.Skender.Stock.Indicators.1.0.1.nupkg" --api-key $(gc G:\Nuget\apikey.txt) --source https://api.nuget.org/v3/index.json  --skip-duplicate

#cp "G:\coldfar_py\sharftrade7\Libs\Stock.Indicators\src\nupkgs\FAkka.Skender.Stock.Indicators.1.0.1*"  "C:\Program Files\dotnet\sdk\8.0.100\FSharp\library-packs"