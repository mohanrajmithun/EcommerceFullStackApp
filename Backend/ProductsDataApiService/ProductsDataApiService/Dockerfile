# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage with SDK
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /source

## Copy and restore SalesAPILibrary
COPY ["SalesAPILibrary/SalesAPILibrary.csproj", "SalesAPILibrary/"]
RUN dotnet restore "SalesAPILibrary/SalesAPILibrary.csproj"

# Copy and build SalesAPILibrary
COPY  SalesAPILibrary/ SalesAPILibrary/
WORKDIR "/source/SalesAPILibrary"
RUN dotnet build "SalesAPILibrary.csproj" -c Release -o /app


# Go back to /src and restore ProductsDataApiService
WORKDIR "/source"
COPY ["./ProductsDataApiService/ProductsDataApiService/ProductsDataApiService.csproj", "ProductsDataApiService/"]
RUN dotnet restore "./ProductsDataApiService/ProductsDataApiService.csproj"

# Copy the rest of the source code and build ProductsDataApiService
COPY ./ProductsDataApiService/ProductsDataApiService ProductsDataApiService/
WORKDIR "/source/ProductsDataApiService"
RUN dotnet build "./ProductsDataApiService.csproj" -c $BUILD_CONFIGURATION -o /app/build

WORKDIR "/source/ProductsDataApiService"

RUN dotnet tool install --global dotnet-ef --version 6.0.0
ENV PATH="$PATH:/root/.dotnet/tools"

RUN dotnet ef database update --project ProductsDataApiService.csproj

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ProductsDataApiService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage for runtime
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProductsDataApiService.dll"]
