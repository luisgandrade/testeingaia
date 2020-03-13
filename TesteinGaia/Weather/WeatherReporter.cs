using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TesteinGaia.Http;
using TesteinGaia.Redis;

namespace TesteinGaia.Weather
{
  public class WeatherReporter
  {

    private readonly RedisConnection _redisConnection;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public WeatherReporter(RedisConnection redisConnection, Uri baseUri, string apiKey)
    {
      _redisConnection = redisConnection;
      _httpClient = HttpClientProviderSingleton.GetHttpClientForBaseUri(baseUri);
      _apiKey = apiKey;
    }


    /// <summary>
    /// Consulta no cache ou na API da OpenWeather a temperatura atual da cidade informada.
    /// </summary>
    /// <param name="city">cidade consultada</param>
    /// <returns>temperatura da cidada consultada</returns>
    /// <remarks>Aqui tiramos vantagem do fato de que a temperatura geralmente não possui mudanças bruscas
    /// de temperatura em curtos períodos de tempo e cacheamos o resultado retornado para cada cidade por 10 minutos.</remarks>
    public async Task<double> GetCurrentTemperatureAtCity(string city)
    {
      if (string.IsNullOrWhiteSpace(city))
        throw new ArgumentNullException(nameof(city));

      var cachedTemperatureAtCity = await _redisConnection.StringReadAndParseAsync<double>($"temperature-{city}");
      if (cachedTemperatureAtCity.Hit)
        return cachedTemperatureAtCity.Value;

      using(var response = await _httpClient.GetAsync($"weather?q={city}&units=metric&appid={_apiKey}")) 
      {
        if (response.IsSuccessStatusCode)
        {
          var responseContents = await response.Content.ReadAsStringAsync();
          var desserializedResponse = JsonConvert.DeserializeObject<WeatherApiResponse>(responseContents, new JsonSerializerSettings
          {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            MissingMemberHandling = MissingMemberHandling.Ignore
          });

          await _redisConnection.StringStoreAsync($"temperature-{city}", desserializedResponse.Main.Temp, TimeSpan.FromMinutes(10));
          return desserializedResponse.Main.Temp;
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
          throw new ApplicationException("O nome da cidade que você informou é inválido.");
        else
          throw new ApplicationException("Houve uma falha de comunicação com o serviço de consulta de temperatura.");
      }


    }

  }
}
