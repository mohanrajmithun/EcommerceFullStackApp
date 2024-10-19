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


WORKDIR "/source"

COPY ./CustomerDataAPI/CustomerDataAPI/CustomerDataAPI.csproj CustomerDataAPI/
RUN dotnet restore "./CustomerDataAPI/CustomerDataAPI.csproj"

COPY ./CustomerDataAPI/CustomerDataAPI CustomerDataAPI/
WORKDIR "/source/CustomerDataAPI"
RUN dotnet build "./CustomerDataAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

WORKDIR "/source/CustomerDataAPI"


# Optional: Install dotnet-ef globally if you need it for migrations at runtime
RUN dotnet tool install --global dotnet-ef --version 6.0.0
ENV PATH="$PATH:/root/.dotnet/tools"


# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CustomerDataAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage for runtime
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /root/.dotnet /root/.dotnet
COPY --from=publish /root/.nuget /root/.nuget


ENTRYPOINT ["dotnet", "CustomerDataAPI.dll"]
