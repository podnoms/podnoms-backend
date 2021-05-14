export ASPNETCORE_ENVIRONMENT=Development &&
    dotnet ef migrations remove \
        --project podnoms-common/podnoms-common.csproj \
        --context PodNomsDbContext
