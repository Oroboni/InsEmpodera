$ErrorActionPreference = "Stop"

dotnet restore .\InsEmpodera.sln
dotnet build .\InsEmpodera.sln --configuration Release --no-restore
dotnet test .\InsEmpodera.sln --configuration Release --no-build --collect:"XPlat Code Coverage"

if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "Todos os testes passaram." -ForegroundColor Green
