# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage with SDK
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /source

COPY Backend/SalesAPILibrary/SalesAPILibrary.csproj SalesAPILibrary/
RUN dotnet restore "SalesAPILibrary/SalesAPILibrary.csproj"


# Copy and build SalesAPILibrary
COPY Backend/SalesAPILibrary/ SalesAPILibrary/
WORKDIR "/source/SalesAPILibrary"
RUN dotnet build "SalesAPILibrary.csproj" -c $BUILD_CONFIGURATION -o /app/build

WORKDIR "/source"
COPY ["Backend/SaleOrderDataService/SaleOrderDataService/SaleOrderDataService.csproj", "SaleOrderDataService/"]
RUN dotnet restore "./SaleOrderDataService/SaleOrderDataService.csproj"


COPY Backend/SaleOrderDataService/SaleOrderDataService SaleOrderDataService/
WORKDIR "/source/SaleOrderDataService"
RUN dotnet build "./SaleOrderDataService.csproj" -c $BUILD_CONFIGURATION -o /app/build


FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SaleOrderDataService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SaleOrderDataService.dll"]

