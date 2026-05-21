using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.Interfaces;
using FGB.Dominio.ObjetoValor;
using ZiggyCreatures.Caching.Fusion;

namespace ConectaSTI.Cache.Servicos;

public class FusionRouteCache : IRouteCache
{
    private static readonly string[] RouteTags = ["routes"];
    private static readonly TimeSpan RouteCacheTtl = TimeSpan.FromSeconds(30);

    private readonly IFusionCache _cache;

    public FusionRouteCache(IFusionCache cache)
    {
        _cache = cache;
    }

    public IReadOnlyList<Rota> GetOrSet(VerboHttp metodo, Func<IReadOnlyList<Rota>> factory)
    {
        return _cache.GetOrSet<IReadOnlyList<Rota>>(
            GetKey(metodo),
            (_, _) => factory()
                .Select(CloneRoute)
                .OrderByDescending(x => GetSpecificityScore(x.Caminho))
                .ToList()
                .AsReadOnly(),
            options: new FusionCacheEntryOptions(RouteCacheTtl),
            tags: RouteTags
        );
    }

    public void Remove(VerboHttp metodo)
    {
        _cache.Remove(GetKey(metodo));
    }

    public void RemoveAll()
    {
        _cache.RemoveByTag(RouteTags);
    }

    private static string GetKey(VerboHttp metodo)
    {
        return $"routes:{metodo}";
    }

    private static int GetSpecificityScore(string template)
    {
        return Normalize(template)
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Count(segmento => !IsParameter(segmento));
    }

    private static bool IsParameter(string segmento)
    {
        return segmento.Length > 2 && segmento.StartsWith("{") && segmento.EndsWith("}");
    }

    private static string Normalize(string caminho)
    {
        return string.IsNullOrWhiteSpace(caminho)
            ? string.Empty
            : caminho.Trim().Trim('/');
    }

    private static Rota CloneRoute(Rota rota)
    {
        return new Rota
        {
            Id = rota.Id,
            CriadoEm = rota.CriadoEm,
            UltimaAlteracao = rota.UltimaAlteracao,
            Nome = rota.Nome,
            Descricao = rota.Descricao,
            SenhaAcesso = rota.SenhaAcesso,
            UsarFluxoMaisAtual = rota.UsarFluxoMaisAtual,
            RateLimit = rota.RateLimit,
            RateLimitRequests = rota.RateLimitRequests,
            RateLimitInterval = rota.RateLimitInterval,
            Idempotencia = rota.Idempotencia,
            JsonSchemaResp = rota.JsonSchemaResp,
            JsonSchemaReq = rota.JsonSchemaReq,
            PipelineVersaoId = rota.PipelineVersaoId,
            PipelineVersao = rota.PipelineVersao,
            Caminho = rota.Caminho,
            Metodo = rota.Metodo
        };
    }
}
