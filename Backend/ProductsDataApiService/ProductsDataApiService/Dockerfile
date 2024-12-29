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


# Go back to /src and restore ProductsDataApiService
WORKDIR "/source"
COPY ["Backend/ProductsDataApiService/ProductsDataApiService/ProductsDataApiService.csproj", "ProductsDataApiService/"]
RUN dotnet restore "./ProductsDataApiService/ProductsDataApiService.csproj"

# Copy the rest of the source code and build ProductsDataApiService
COPY Backend/ProductsDataApiService/ProductsDataApiService ProductsDataApiService/
WORKDIR "/source/ProductsDataApiService"
RUN dotnet build "./ProductsDataApiService.csproj" -c $BUILD_CONFIGURATION -o /app/build


# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ProductsDataApiService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage for runtime
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProductsDataApiService.dll"]
