# Worksuite.Phnx

`Worksuite.Phnx.Audit.EF` is forked from [Phnx.Audit.EF](https://github.com/phoenix-apps/EF-Audit).

## Build

```
dotnet build src/Phnx.Audit.EF/Phnx.Audit.EF.csproj --configuration Release
```

## Pack

```
dotnet pack src/Phnx.Audit.EF/Phnx.Audit.EF.csproj --configuration Release
```

## Push

```
dotnet nuget push --source "https://pkgs.dev.azure.com/totalmobile/_packaging/TotalMobile/nuget/v3/index.json" --api-key az ./src/Phnx.Audit.EF/bin/Release/Worksuite.Phnx.Audit.EF.1.0.0.nupkg --interactive
```
