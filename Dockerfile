FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["TradingView.Signals.Api/TradingView.Signals.Api.csproj", "TradingView.Signals.Api/"]
RUN dotnet restore "TradingView.Signals.Api/TradingView.Signals.Api.csproj"
COPY . .
WORKDIR "/src/TradingView.Signals.Api"
RUN dotnet build "TradingView.Signals.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TradingView.Signals.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TradingView.Signals.Api.dll"]
