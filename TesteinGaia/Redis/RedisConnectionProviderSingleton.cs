using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TesteinGaia.Redis
{
  public class RedisConnectionProviderSingleton : IDisposable
  {
    private const int MAXIMUM_CONNECTION_RETRIES = 3;

    private ConnectionMultiplexer _connectionMultiplexer;

    private readonly RedisConfig _redisConfig;

    public RedisConnectionProviderSingleton(RedisConfig redisConfig)
    {
      _redisConfig = redisConfig;
      _connectionMultiplexer = ConnectionMultiplexer.Connect(new ConfigurationOptions
      {
        AllowAdmin = true,
        EndPoints =
        {
            { _redisConfig.Host, _redisConfig.Port }
        }
      });
    }

    private void OnConnectionFailed(object ev, ConnectionFailedEventArgs args)
    {
      _connectionMultiplexer.Close();
      _connectionMultiplexer = null;
    }

    public RedisConnection GetDefaultRedisDatabase() => new RedisConnection(_connectionMultiplexer.GetDatabase(_redisConfig.Database));

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          // TODO: dispose managed state (managed objects).
        }

        if(_connectionMultiplexer != null)
          _connectionMultiplexer.Dispose();

        disposedValue = true;
      }
    }

    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    ~RedisConnectionProviderSingleton()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(false);
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
      // TODO: uncomment the following line if the finalizer is overridden above.
      // GC.SuppressFinalize(this);
    }
    #endregion

  }
}
