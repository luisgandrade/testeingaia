using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TesteinGaia.Http;
using TesteinGaia.Redis;

namespace TesteinGaia.MusicRecommendations
{
  public class SpotifyMusicRecommender
  {

    private readonly RedisConnection _redisConnection;
    private readonly HttpClient _httpClient;

    private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

    public SpotifyMusicRecommender(RedisConnection redisConnection, Uri baseUri, string bearerToken)
    {
      _redisConnection = redisConnection;
      _httpClient = HttpClientProviderSingleton.GetHttpClientForBaseUri(baseUri);

      _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
    }

    private string ChooseGenreBasedOnTemperature(double temperature)
    {
      if (temperature < 10)
        return "classical";
      else if (temperature < 25)
        return "rock";
      else return "pop";
    }

    /// <summary>
    /// Busca uma playlist de recomendações de músicas no cache ou na API do Spotify baseado na temperatura informada.
    /// </summary>
    /// <param name="temperature">temperatura</param>
    /// <param name="playlistSize">tamanho da playlist a ser gerada</param>
    /// <returns>uma playlist, contendo nome do artista e música</returns>
    /// <remarks>As playlistas para cada gênero são cacheadas por 5 minutos</remarks>
    public async Task<IList<string>> GetRecommendationsBasedOnTemperature(double temperature, int playlistSize = 20)
    {
      
      var genre = ChooseGenreBasedOnTemperature(temperature);

      var cachedplaylistForGenre = await _redisConnection.StringReadAndParseAsync<IList<string>>($"recommendations-{genre}");
      if (cachedplaylistForGenre.Hit)
        return cachedplaylistForGenre.Value;

      using (var response = await _httpClient.GetAsync($"recommendations?seed_genres={genre}&limit={playlistSize}"))
      {
        if (response.IsSuccessStatusCode)
        {
          var responseContents = await response.Content.ReadAsStringAsync();
          var deserializedResponse = JsonConvert.DeserializeObject<SpotifyRecommendationsResponse>(responseContents, new JsonSerializerSettings
          {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            MissingMemberHandling = MissingMemberHandling.Ignore
          });

          var playlist = deserializedResponse.Tracks.Select(tr => $"{string.Join(",", tr.Artists.Select(art => art.Name))} - {tr.Name}").ToList();
          await _redisConnection.StringStoreAsync($"recommendations-{genre}", playlist, TimeSpan.FromMinutes(5));
          return playlist;

        }
        else
          throw new ApplicationException("Houve uma falha de comunicação com o serviço de recomendação de músicas.");
      }
    }
  }
}
