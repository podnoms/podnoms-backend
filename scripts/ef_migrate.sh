export ASPNETCORE_ENVIRONMENT=Development &&
    dotnet ef migrations add $1 \
        --project podnoms-common/podnoms-common.csproj \
        --context PodNomsDbContext
