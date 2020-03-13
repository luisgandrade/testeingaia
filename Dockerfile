FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["TesteinGaia/TesteinGaia.csproj", "TesteinGaia/"]
RUN dotnet restore "TesteinGaia/TesteinGaia.csproj"
COPY . .
WORKDIR "/src/TesteinGaia"
RUN dotnet build "TesteinGaia.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TesteinGaia.csproj" -c Release -o /app/publish

FROM base AS final
ARG OPEN_WEATHER_API_KEY
ARG SPOTIFY_BEARER_TOKEN
WORKDIR /app
COPY --from=publish /app/publish .
RUN sed -i "s/<<open_weather_api_key>>/$OPEN_WEATHER_API_KEY/g" appsettings.json
RUN sed -i "s/<<spotify_bearer_token>>/$SPOTIFY_BEARER_TOKEN/g" appsettings.json
ENTRYPOINT ["dotnet", "TesteinGaia.dll"]