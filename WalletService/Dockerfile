FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Shared.Models/Shared.Models.csproj Shared.Models/
COPY WalletService/WalletService.csproj WalletService/
RUN dotnet restore WalletService/WalletService.csproj

COPY Shared.Models/ Shared.Models/
COPY WalletService/ WalletService/
RUN dotnet publish WalletService/WalletService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "WalletService.dll"]
