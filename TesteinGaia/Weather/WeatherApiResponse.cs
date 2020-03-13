using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TesteinGaia.Weather
{
  /// <summary>
  /// POCO que representa parcial a resposta da API à query sobre o clima de uma cidade.
  /// Somente atributos relevantes foram codificados aqui.
  /// </summary>
  public class WeatherApiResponse
  {
    public MainSection Main { get; set; }
  }
}
