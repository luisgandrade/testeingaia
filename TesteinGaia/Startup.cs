using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TesteinGaia.MusicRecommendations;
using TesteinGaia.Redis;
using TesteinGaia.Weather;

namespace TesteinGaia
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {

      services.AddSingleton(sc => new RedisConnectionProviderSingleton(Configuration.GetSection("RedisConfig").Get<RedisConfig>()));
      services.AddScoped(sc =>
      {
        var redisDb = sc.GetService<RedisConnectionProviderSingleton>().GetDefaultRedisDatabase();
        var weatherUrl = Configuration.GetSection("OpenWeatherApiConfig").GetSection("Uri").Value;
        var weatherApiKey = Configuration.GetSection("OpenWeatherApiConfig").GetSection("Key").Value;
        return new WeatherReporter(redisDb, new Uri(weatherUrl), weatherApiKey);
      });

      services.AddScoped(sc =>
      {
        var redisDb = sc.GetService<RedisConnectionProviderSingleton>().GetDefaultRedisDatabase();
        var spotifyUrl = Configuration.GetSection("SpotifyApiConfig").GetSection("Uri").Value;
        var spotifyBearerToken = Configuration.GetSection("SpotifyApiConfig").GetSection("BearerToken").Value;
        return new SpotifyMusicRecommender(redisDb, new Uri(spotifyUrl), spotifyBearerToken);
      });

      var cultureInfo = CultureInfo.CreateSpecificCulture("en-US");
      CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
      CultureInfo.DefaultThreadCurrentCulture = cultureInfo;

      services.AddControllers();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      app.UseRouting();

      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });
    }
  }
}
