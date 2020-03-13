using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TesteinGaia.Redis
{
  /// <summary>
  /// Representa uma conexão com o Redis e disponibiliza métodos ajudantes
  /// para aliviar a diferença de impedância entre aplicação e Redis.
  /// </summary>
  public class RedisConnection
  {

    private readonly IDatabase _redisDb;

    public RedisConnection(IDatabase redisDb)
    {
      _redisDb = redisDb;
    }


    public async Task StringStoreAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
      if (string.IsNullOrWhiteSpace(key))
        throw new ArgumentNullException(nameof(key));

      string serializedObject;
      var valueType = typeof(T);

      if (valueType.IsPrimitive || valueType == typeof(string) || valueType == typeof(decimal) || valueType == typeof(DateTime))
        serializedObject = value.ToString();
      else
        serializedObject = JsonConvert.SerializeObject(value);


      await _redisDb.StringSetAsync(key, serializedObject, expiration);
    }

    public async Task<CacheResult<T>> StringReadAndParseAsync<T>(string key)
    {
      if (string.IsNullOrWhiteSpace(key))
        throw new ArgumentNullException(nameof(key));

      T deserializedObject = default(T);

      var valueAsString = await _redisDb.StringGetAsync(key);
      if (valueAsString.HasValue)
        deserializedObject = JsonConvert.DeserializeObject<T>(valueAsString);

      if (typeof(T).IsValueType && default(T).Equals(deserializedObject) || !typeof(T).IsValueType && deserializedObject == null)
        return CacheResult<T>.CacheMiss();

      return CacheResult<T>.CacheHit(deserializedObject);
    }
  }
}
