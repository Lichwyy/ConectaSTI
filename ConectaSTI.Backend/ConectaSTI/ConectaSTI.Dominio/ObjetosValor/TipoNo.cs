using System;
using System.Collections.Generic;
using System.Text;

namespace ConectaSTI.Dominio.ObjetosValor
{
    public enum TipoNo
    {
        Requisicao = 1,
        FuncaoJS = 2, // talvez mais coisas no futuro, mas por enquanto só tem essas
        SalvarStorage = 3,
        PegarStorage = 4
    }
}
