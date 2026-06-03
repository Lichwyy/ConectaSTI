using System.Text.Json;
using System.Text.RegularExpressions;

namespace ConectaSTI.Executor.Servicos;

internal static class JsonPlaceholderInterpolator
{
    private static readonly Regex PlaceholderRegex = new Regex(@"\{\{\s*([^{}]+?)\s*\}\}", RegexOptions.Compiled);

    public static string Interpolar(string textoAlvo, object dadoAnterior)
    {
        if (string.IsNullOrWhiteSpace(textoAlvo) || dadoAnterior == null)
            return textoAlvo;

        try
        {
            string jsonString = dadoAnterior is string str ? str : SerializeSafe(dadoAnterior);
            using JsonDocument document = JsonDocument.Parse(jsonString);

            return PlaceholderRegex.Replace(textoAlvo, match =>
            {
                string caminho = match.Groups[1].Value.Trim();
                if (!TryResolveJsonPath(document.RootElement, caminho, out JsonElement valor))
                {
                    return match.Value;
                }

                return valor.ValueKind == JsonValueKind.String
                    ? valor.GetString()
                    : valor.GetRawText();
            });
        }
        catch
        {
            return textoAlvo;
        }
    }

    private static bool TryResolveJsonPath(JsonElement root, string caminho, out JsonElement valor)
    {
        valor = root;

        if (string.IsNullOrWhiteSpace(caminho))
        {
            return false;
        }

        if (caminho.StartsWith("$."))
        {
            caminho = caminho.Substring(2);
        }

        foreach (string parte in caminho.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            string segmento = parte;

            while (segmento.Length > 0)
            {
                int bracketIndex = segmento.IndexOf('[');
                string nomePropriedade = bracketIndex >= 0 ? segmento.Substring(0, bracketIndex) : segmento;

                if (!string.IsNullOrWhiteSpace(nomePropriedade))
                {
                    if (valor.ValueKind != JsonValueKind.Object || !valor.TryGetProperty(nomePropriedade, out valor))
                    {
                        return false;
                    }
                }

                if (bracketIndex < 0)
                {
                    break;
                }

                int bracketEnd = segmento.IndexOf(']', bracketIndex);
                if (bracketEnd < 0)
                {
                    return false;
                }

                string indexText = segmento.Substring(bracketIndex + 1, bracketEnd - bracketIndex - 1);
                if (!int.TryParse(indexText, out int index) || valor.ValueKind != JsonValueKind.Array)
                {
                    return false;
                }

                if (index < 0 || index >= valor.GetArrayLength())
                {
                    return false;
                }

                valor = valor.EnumerateArray().ElementAt(index);
                segmento = segmento.Substring(bracketEnd + 1);
            }
        }

        return true;
    }

    private static string SerializeSafe(object valor)
    {
        if (valor == null)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Serialize(valor);
        }
        catch
        {
            return valor.ToString();
        }
    }
}
