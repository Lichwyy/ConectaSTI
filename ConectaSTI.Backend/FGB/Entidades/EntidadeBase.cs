using FGB.Servicos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FGB.Entidades
{
    public class EntidadeBase
    {
    public long Id { get; set; }

    public DateTime? CriadoEm { get; set; }

    public DateTime? UltimaAlteracao { get; set; }
    }
}
