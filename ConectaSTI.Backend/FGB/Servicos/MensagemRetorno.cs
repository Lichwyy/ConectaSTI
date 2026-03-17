using System;
using System.Collections.Generic;
using System.Linq;

namespace FGB.Servicos
{
    public class MensagemRetorno
    {
        public string Mensagem { get; set; }
        public bool Erro { get; set; }
        public Exception Exception { get; set; }

        public MensagemRetorno() { }

        public MensagemRetorno(string mensagem, bool erro = false)
        {
            Mensagem = mensagem;
            Erro = erro;
        }

        public MensagemRetorno(Exception exception)
        {
            Exception = exception;
            Mensagem = exception.Message;
            Erro = true;
        }
    }

    public class ListaMensagens : List<MensagemRetorno>
    {
        public void Add(Exception exception)
        {
            Add(new MensagemRetorno(exception));
        }

        public void Add(string mensagem, bool erro = false)
        {
            Add(new MensagemRetorno(mensagem, erro));
        }

        public bool HasErro()
        {
            return this.Any(m => m.Erro);
        }
    }
}
