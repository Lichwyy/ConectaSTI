using System;
using System.Collections.Generic;
using System.Text;

namespace ConectaSTI.Dominio.ObjetosValor
{
    public enum StatusOperacao
    {
        Sucesso,
        Falha,
        EmExecucao,
        Retryando,
        Ignorado
    }
}
