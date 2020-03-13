# Teste inGaia
#### Candidato: Luís Gabriel de Andrade

## Docker:
- Em `docker-compose.yml`, substituir as variáves de argumento `<<chave de api do open weather>>` e `<<token de autenticação do spotify>>` por seus respectivos valores;
- Buildar com `docker-compose build`;
- Rodar com `docker-compose up -d`.

## Visual Studio:
- **Essa abordagem necessita da instalação do SDK do .NET Core 3+ e do Redis**
  - [SDK do .NET Core](https://dotnet.microsoft.com/download)
  - [Redis para Windows](https://github.com/dmajkic/redis/downloads)
  - [Redis para Linux](https://redis.io/download)
- Em `appsettings.json` (para compilação em modo release) ou `appsettings.development.json`, (para compilação em modo debug) substituir as valores  `<<open_weather_api_key>>` e `<<spotify_bearer_token>>` por seus respectivos valores. O host e porta do Redis também devem ser editados para considerar configurações diferentes.
