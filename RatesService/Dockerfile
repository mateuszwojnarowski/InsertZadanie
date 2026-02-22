FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Shared.Models/Shared.Models.csproj Shared.Models/
COPY RatesService/RatesService.csproj RatesService/
RUN dotnet restore RatesService/RatesService.csproj

COPY Shared.Models/ Shared.Models/
COPY RatesService/ RatesService/
RUN dotnet publish RatesService/RatesService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "RatesService.dll"]
