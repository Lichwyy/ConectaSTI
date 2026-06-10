using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConectaSTI.Dominio.DTOs;

public class EntradaFluxoDTO
{
    [JsonPropertyName("routeParams")]
    public Dictionary<string, string> RouteParams { get; set; } = new();

    [JsonPropertyName("queryParams")]
    public Dictionary<string, string> QueryParams { get; set; } = new();

    [JsonPropertyName("body")]
    public object Body { get; set; }

    public bool TemDados()
    {
        return (RouteParams?.Count ?? 0) > 0
            || (QueryParams?.Count ?? 0) > 0
            || !BodyEstaVazio();
    }

    private bool BodyEstaVazio()
    {
        if (Body == null)
        {
            return true;
        }

        if (Body is string texto)
        {
            return string.IsNullOrWhiteSpace(texto);
        }

        if (Body is JsonElement json)
        {
            return json.ValueKind switch
            {
                JsonValueKind.Undefined => true,
                JsonValueKind.Null => true,
                JsonValueKind.Object => !json.EnumerateObject().Any(),
                JsonValueKind.Array => json.GetArrayLength() == 0,
                JsonValueKind.String => string.IsNullOrWhiteSpace(json.GetString()),
                _ => false
            };
        }

        return false;
    }
}
