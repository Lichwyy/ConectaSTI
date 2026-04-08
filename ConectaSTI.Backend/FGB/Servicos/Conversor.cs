using FGB.Dominio.Interfaces.Utilitarios;
using FGB.Dominio.ObjetoValor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FGB.Dominio.Servicos
{
    public class Conversor : IConverter
    {
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

        public T Desserializar<T>(string estrutura, TipoSerializacao tipoSerializacao = TipoSerializacao.CamelCase)
        {
            var deserializadorSettings = new JsonSerializerOptions();
            switch (tipoSerializacao)
            {
                case TipoSerializacao.CamelCase:
                    deserializadorSettings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    break;
                case TipoSerializacao.UnderscoreCase:
                    deserializadorSettings.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                    break;
                default:
                    break;
            }

            return JsonSerializer.Deserialize<T>(estrutura, deserializadorSettings)!;
        }
    }
}
