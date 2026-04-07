using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;

namespace FGB.Dominio.ObjetoValor
{
    public class RequisicaoHttp
    {
        private string _urlRequest, _url;
        private object _queryParams;

        public RequisicaoHttp()
        {
            Headers = new WebHeaderCollection();
            TimeoutSegundos = 100;
            Accept = AcceptProxy.Json;
            ContentType = AcceptProxy.Json;
        }

        public string UrlRequest
        {
            get
            {
                if (string.IsNullOrEmpty(_urlRequest))
                {
                    _urlRequest = Url;
                    if (QueryParams is string q)
                    {
                        if (!string.IsNullOrWhiteSpace(q))
                        {
                            if (!q.StartsWith("?", StringComparison.Ordinal))
                            {
                                _urlRequest += "?";
                            }

                            _urlRequest += q;
                        }
                    }
                    else if (QueryParams != null)
                    {
                        var properties = PegarValorPropriedades();
                        var parameters = string.Join("&", properties.ToArray());
                        _urlRequest = $"{Url}?{parameters}";
                    }
                }
                return _urlRequest;
            }
        }
        public string Url { get => _url; set { _url = value; _urlRequest = ""; } }
        public string Rota { get; set; }
        public VerboHttp Verbo { get; set; }
        public AcceptProxy Accept { get; set; }
        public AcceptProxy ContentType { get; set; }
        public WebHeaderCollection Headers { get; set; }
        public object QueryParams { get => _queryParams; set { _queryParams = value; _urlRequest = ""; } }
        public object Body { get; set; }
        public string BodyRaw { get; set; }
        public int TimeoutSegundos { get; set; }
        public bool IgnorarCertificadoSsl { get; set; }

        private IEnumerable<string> PegarValorPropriedades()
        {
            if (QueryParams is Dictionary<string, string> dict)
            {
                return from p in dict.Keys
                       let valor = dict[p]
                       where valor != null
                       select $"{p}={WebUtility.UrlEncode(valor)}";
            }

            return from p in QueryParams.GetType().GetProperties()
                   let valor = p.GetValue(QueryParams)
                   where valor != null
                   select p.Name + "=" + (
                       valor is double or float or decimal
                       ? WebUtility.UrlEncode(Convert.ToDouble(valor).ToString("G", CultureInfo.InvariantCulture))
                       : WebUtility.UrlEncode(valor.ToString())
                   );
        }
    }
}
