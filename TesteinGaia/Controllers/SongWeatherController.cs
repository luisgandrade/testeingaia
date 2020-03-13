using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TesteinGaia.MusicRecommendations;
using TesteinGaia.Weather;

namespace TesteinGaia.Controllers
{
  [ApiController]
  [Route("clima-sonoro")]
  public class SongWeatherController : ControllerBase
  {
    private readonly WeatherReporter _weatherReporter;
    private readonly SpotifyMusicRecommender _spotifyMusicRecommender;

    public SongWeatherController(WeatherReporter weatherReporter, SpotifyMusicRecommender spotifyMusicRecommender)
    {
      _weatherReporter = weatherReporter;
      _spotifyMusicRecommender = spotifyMusicRecommender;
    }

    [HttpGet("{city}")]
    public async Task<IActionResult> Get(string city)
    {
      if (string.IsNullOrWhiteSpace(city))
        return BadRequest("Informe o nome de uma cidade");

      try
      {
        var temperature = await _weatherReporter.GetCurrentTemperatureAtCity(city);
        var recommendations = await _spotifyMusicRecommender.GetRecommendationsBasedOnTemperature(temperature);

        return Ok(recommendations);
      }
      catch(ApplicationException ae)
      {
        return BadRequest(ae.Message);
      }
      catch(Exception)
      {
        return StatusCode(502);
      }
    }
  }
}
