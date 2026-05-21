using System;
using System.Threading.RateLimiting;

namespace ConectaSTI.Dominio.Interfaces;

public interface IRateLimiterCache
{
    FixedWindowRateLimiter GetOrCreate(string key, int permitLimit, TimeSpan window);
    void Remove(string key);
}
