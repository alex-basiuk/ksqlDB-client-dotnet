docker-compose up -d

dotnet build -c Release KsqlDb.Client.IntegrationTests\KsqlDb.Client.IntegrationTests.csproj

.\setup-ksqldb-test-env.ps1

dotnet test --no-build KsqlDb.Client.IntegrationTests\KsqlDb.Client.IntegrationTests.csproj

docker-compose down