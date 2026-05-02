using System;
using System.Collections.Generic;
using System.Text;
using FGB.Dominio.ObjetoValor;
using FGB.Servicos;

namespace FGB.Dominio.Interfaces.Utilitarios
{
    public interface IConverter
    {
        ListaMensagens Mensagens { get; }
        string Serializar(object valores, TipoSerializacao tipoSerializacao = TipoSerializacao.CamelCase, bool indent = false);
        T Desserializar<T>(string estrutura, TipoSerializacao tipoSerializacao = TipoSerializacao.CamelCase);
    }
}
