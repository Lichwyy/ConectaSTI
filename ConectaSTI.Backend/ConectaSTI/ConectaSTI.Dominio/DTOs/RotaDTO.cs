using ConectaSTI.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConectaSTI.Dominio.DTOs
{
    public class RotaDTO
    {
        public Rota Rota { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new();
        public string Body { get; set; }
        public Dictionary<string, string> QueryParams { get; set; } = new();
    }
}
