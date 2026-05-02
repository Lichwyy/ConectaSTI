using ConectaSTI.Dominio.Entidades;
using FGB.IRepositorios;
using FGB.Servicos;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConectaSTI.Dominio.Servicos
{
    public class ServicoStorage : ServicoCrud<Storage>
    {
        public ServicoStorage(IRepositorioSessao repositorio) : base(repositorio)
        {
        }
    }
}
