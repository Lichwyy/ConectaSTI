using ConectaSTI.Rotas.Interfaces;
using System.Text.Json;

namespace ConectaSTI.Rotas.Servicos
{
    public class JsonValidator : ISchemaValidator
    {
        public List<string> ValidateJsonAgainstSchema(string json, string schema)
        {
            List<string> errors = new List<string>();

            JsonElement instancia;
            JsonElement schemaElement;

            try
            {
                using JsonDocument jsonDocument = JsonDocument.Parse(json);
                instancia = jsonDocument.RootElement.Clone();
            }
            catch (JsonException ex)
            {
                errors.Add($"JSON da requisicao invalido: {ex.Message}");
                return errors;
            }

            try
            {
                using JsonDocument schemaDocument = JsonDocument.Parse(schema);
                schemaElement = schemaDocument.RootElement.Clone();
            }
            catch (JsonException ex)
            {
                errors.Add($"Schema JSON invalido na rota: {ex.Message}");
                return errors;
            }

            ValidateNode("$", instancia, schemaElement, errors);
            return errors;
        }

        private static void ValidateNode(string path, JsonElement instancia, JsonElement schema, IList<string> errors)
        {
            if (schema.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            if (schema.TryGetProperty("type", out JsonElement typeElement))
            {
                string expectedType = typeElement.GetString();
                if (!MatchesType(instancia, expectedType))
                {
                    errors.Add($"{path}: esperado tipo '{expectedType}'.");
                    return;
                }
            }

            if (schema.TryGetProperty("enum", out JsonElement enumElement) && enumElement.ValueKind == JsonValueKind.Array)
            {
                bool encontrado = enumElement.EnumerateArray().Any(opcao => JsonElementEqualityComparer.Instance.Equals(opcao, instancia));
                if (!encontrado)
                {
                    errors.Add($"{path}: valor fora do enum permitido.");
                    return;
                }
            }

            if (instancia.ValueKind == JsonValueKind.Object)
            {
                ValidateRequired(path, instancia, schema, errors);

                if (schema.TryGetProperty("properties", out JsonElement propertiesElement) && propertiesElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (JsonProperty propriedadeSchema in propertiesElement.EnumerateObject())
                    {
                        if (instancia.TryGetProperty(propriedadeSchema.Name, out JsonElement valorPropriedade))
                        {
                            ValidateNode($"{path}.{propriedadeSchema.Name}", valorPropriedade, propriedadeSchema.Value, errors);
                        }
                    }
                }
            }

            if (instancia.ValueKind == JsonValueKind.Array &&
                schema.TryGetProperty("items", out JsonElement itemsElement) &&
                itemsElement.ValueKind == JsonValueKind.Object)
            {
                int index = 0;
                foreach (JsonElement item in instancia.EnumerateArray())
                {
                    ValidateNode($"{path}[{index}]", item, itemsElement, errors);
                    index++;
                }
            }
        }

        private static void ValidateRequired(string path, JsonElement instancia, JsonElement schema, IList<string> errors)
        {
            if (!schema.TryGetProperty("required", out JsonElement requiredElement) || requiredElement.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            foreach (JsonElement requiredItem in requiredElement.EnumerateArray())
            {
                string nomePropriedade = requiredItem.GetString();
                if (!instancia.TryGetProperty(nomePropriedade, out _))
                {
                    errors.Add($"{path}: propriedade obrigatoria '{nomePropriedade}' ausente.");
                }
            }
        }

        private static bool MatchesType(JsonElement instancia, string expectedType)
        {
            return expectedType switch
            {
                "object" => instancia.ValueKind == JsonValueKind.Object,
                "array" => instancia.ValueKind == JsonValueKind.Array,
                "string" => instancia.ValueKind == JsonValueKind.String,
                "number" => instancia.ValueKind == JsonValueKind.Number,
                "integer" => instancia.ValueKind == JsonValueKind.Number && instancia.TryGetInt64(out _),
                "boolean" => instancia.ValueKind is JsonValueKind.True or JsonValueKind.False,
                "null" => instancia.ValueKind == JsonValueKind.Null,
                _ => true
            };
        }

        private sealed class JsonElementEqualityComparer : IEqualityComparer<JsonElement>
        {
            public static JsonElementEqualityComparer Instance { get; } = new JsonElementEqualityComparer();

            public bool Equals(JsonElement x, JsonElement y)
            {
                return x.ValueKind == y.ValueKind && x.GetRawText() == y.GetRawText();
            }

            public int GetHashCode(JsonElement obj)
            {
                return HashCode.Combine(obj.ValueKind, obj.GetRawText());
            }
        }
    }
}
