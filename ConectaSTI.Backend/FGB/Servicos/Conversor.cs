using FGB.Dominio.Interfaces.Utilitarios;
using FGB.Dominio.ObjetoValor;
using FGB.Servicos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FGB.Dominio.Servicos
{
    public class Conversor : IConverter
    {
        public ListaMensagens Mensagens { get; private set; } = new ListaMensagens();
        public string Serializar(object valores, TipoSerializacao tipoSerializacao = TipoSerializacao.CamelCase, bool indent = false)
        {
            var serializadorSettings = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = indent
            };

            switch (tipoSerializacao)
            {
                case TipoSerializacao.CamelCase:
                    serializadorSettings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    break;
                case TipoSerializacao.UnderscoreCase:
                    serializadorSettings.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                    break;
            }

            return JsonSerializer.Serialize(valores, serializadorSettings);
        }

        public T? Desserializar<T>( string estrutura, TipoSerializacao tipoSerializacao = TipoSerializacao.CamelCase)
        {
            if (string.IsNullOrWhiteSpace(estrutura))
            {
                Mensagens.Add("A estrutura para desserialização não pode ser vazia.", true);
                return default;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            switch (tipoSerializacao)
            {
                case TipoSerializacao.CamelCase:
                    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    break;

                case TipoSerializacao.UnderscoreCase:
                    options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                    break;
            }

            try
            {
                var resultado = JsonSerializer.Deserialize<T>(estrutura, options);

                if (resultado is null)
                {
                    Mensagens.Add($"Não foi possível desserializar o conteúdo para o tipo {typeof(T).Name}.", true);
                    return default;
                }

                return resultado;
            }
            catch (JsonException ex)
            {
                Mensagens.Add(ex);
                return default;
            }
            catch (Exception ex)
            {
                Mensagens.Add(ex);
                return default;
            }
        }
    }
}
