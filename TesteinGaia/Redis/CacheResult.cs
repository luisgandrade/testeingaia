using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TesteinGaia.Redis
{
  public class CacheResult<TCachedValue>
  {
    public TCachedValue Value { get; private set; }

    public bool Hit { get; private set; }

    public bool Miss
    {
      get
      {
        return !Hit;
      }
    }


    private CacheResult()
    {

    }

    internal static CacheResult<TCachedValue> CacheHit(TCachedValue value)
    {
      return new CacheResult<TCachedValue>
      {
        Hit = true,
        Value = value
      };
    }

    internal static CacheResult<TCachedValue> CacheMiss()
    {
      return new CacheResult<TCachedValue>
      {
        Hit = false,
      };
    }
  }
}
