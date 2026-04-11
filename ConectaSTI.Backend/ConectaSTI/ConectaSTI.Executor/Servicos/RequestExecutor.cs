using System;
using System.Collections.Generic;
using System.Text;
using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.Interfaces;
using FGB.Dominio.Interfaces.Utilitarios;
using FGB.Dominio.ObjetoValor;
using FGB.IRepositorios;

namespace ConectaSTI.Executor.Servicos
{
    public class RequestExecutor : IRequestExecutor
    {
        private readonly IRepositorioConsulta _repositorioConsulta;
        private readonly IRequest _request;
        private readonly IConverter _converter;

        public RequestExecutor(IRepositorioConsulta repositorioConsulta,  IRequest request, IConverter converter)
        {
            _repositorioConsulta = repositorioConsulta;
            _request = request;
            _converter = converter;
        }
        
        
        public object EnviarRequisicao(No nozinho)
        {
            RequisicaoHttp request =  new RequisicaoHttp();
            
            EndPoint endpointzinho = _repositorioConsulta.Consulta<EndPoint>(x => x.Id == nozinho.EndPointId).FirstOrDefault();
            Integracao integracaozinha = _repositorioConsulta.Consulta<Integracao>(x => x.Id == endpointzinho.IntegracaoId).FirstOrDefault();
            
            // Montagem do Header
            if (nozinho.Headers != null)
            {
                Dictionary<string, string> headerDic = _converter.Desserializar<Dictionary<string, string>>(nozinho.Headers);
                
                foreach (var item in headerDic)
                {
                    request.Headers.Add(item.Key, item.Value);
                }
            }

            if (integracaozinha != null)
            {
                var token = integracaozinha.Token;

                if (token != null)
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }
            }
            
            // Montagem do Body
            if (nozinho.Body != null)
            {
                request.Body = _converter.Desserializar<object>(nozinho.Body);
                
            }
            
            // Montagem do Verbo
            request.Verbo = endpointzinho.Verbo;
            
            var cu = _request.Fetch<object>(request);

            return cu;
        }
    }
}
