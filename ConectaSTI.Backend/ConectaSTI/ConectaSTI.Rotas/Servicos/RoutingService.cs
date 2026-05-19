using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Rotas.Interfaces;
using FGB.Dominio.ObjetoValor;
using FGB.IRepositorios;

namespace ConectaSTI.Rotas.Servicos
{
    public class RoutingService : IRoutingService
    {
        private readonly IRepositorioConsulta _consulta;

        public RoutingService(IRepositorioConsulta consulta)
        {
            _consulta = consulta;
        }

        public Rota GetRoute(VerboHttp metodo, string caminho, out Dictionary<string, string> parametros)
        {
            parametros = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var rotas = _consulta.Consulta<Rota>(x => x.Metodo == metodo).ToList();
            foreach (var rota in rotas.OrderByDescending(x => GetSpecificityScore(x.Caminho)))
            {
                if (TryMatch(rota.Caminho, caminho, out var parametrosEncontrados))
                {
                    parametros = parametrosEncontrados;
                    return rota;
                }
            }

            return null;
        }

        private static bool TryMatch(string template, string requestPath, out Dictionary<string, string> parametros)
        {
            parametros = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var templateSegmentos = Normalize(template).Split('/', StringSplitOptions.RemoveEmptyEntries);
            var requestSegmentos = Normalize(requestPath).Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (templateSegmentos.Length != requestSegmentos.Length)
            {
                return false;
            }

            for (int i = 0; i < templateSegmentos.Length; i++)
            {
                string templateSegmento = templateSegmentos[i];
                string requestSegmento = requestSegmentos[i];

                if (IsParameter(templateSegmento))
                {
                    string nomeParametro = templateSegmento[1..^1];
                    parametros[nomeParametro] = requestSegmento;
                    continue;
                }

                if (!string.Equals(templateSegmento, requestSegmento, StringComparison.OrdinalIgnoreCase))
                {
                    parametros.Clear();
                    return false;
                }
            }

            return true;
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
    }
}
