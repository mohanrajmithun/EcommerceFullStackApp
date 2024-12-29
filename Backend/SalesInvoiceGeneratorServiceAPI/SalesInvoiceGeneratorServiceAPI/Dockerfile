# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage with SDK
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /source

# Copy and restore SalesAPILibrary
COPY Backend/SalesAPILibrary/SalesAPILibrary.csproj SalesAPILibrary/
RUN dotnet restore "SalesAPILibrary/SalesAPILibrary.csproj"


# Copy and build SalesAPILibrary
COPY Backend/SalesAPILibrary/ SalesAPILibrary/
WORKDIR "/source/SalesAPILibrary"
RUN dotnet build "SalesAPILibrary.csproj" -c $BUILD_CONFIGURATION -o /app/build


WORKDIR "/source"
COPY Backend/SalesInvoiceGeneratorServiceAPI/SalesInvoiceGeneratorServiceAPI/SalesInvoiceGeneratorServiceAPI.csproj SalesInvoiceGeneratorServiceAPI/
RUN dotnet restore "./SalesInvoiceGeneratorServiceAPI/SalesInvoiceGeneratorServiceAPI.csproj"


COPY Backend/SalesInvoiceGeneratorServiceAPI/SalesInvoiceGeneratorServiceAPI SalesInvoiceGeneratorServiceAPI/
WORKDIR "/source/SalesInvoiceGeneratorServiceAPI"
RUN dotnet build "./SalesInvoiceGeneratorServiceAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build


FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SalesInvoiceGeneratorServiceAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SalesInvoiceGeneratorServiceAPI.dll"]