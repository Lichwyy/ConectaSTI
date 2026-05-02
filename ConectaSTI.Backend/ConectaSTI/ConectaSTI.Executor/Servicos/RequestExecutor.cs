using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.Interfaces;
using FGB.Dominio.Interfaces.Utilitarios;
using FGB.Dominio.ObjetoValor;
using FGB.IRepositorios;
using FGB.Servicos;

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
        
        public RespostaHttp<object> EnviarRequisicao(No nozinho)
        {
            var respostaRequisicao = new RespostaHttp<object>();
            
            if (nozinho == null)
            {
                respostaRequisicao.Status = 500;
                respostaRequisicao.Retorno.Add(new MensagemRetorno("O no nao pode ser nulo", true));
                return respostaRequisicao;
            }
            
            RequisicaoHttp request =  new RequisicaoHttp();
            
            EndPoint endpointzinho = _repositorioConsulta.Consulta<EndPoint>(x => x.Id == nozinho.EndPointId).FirstOrDefault();
            
            if (endpointzinho == null)
            {
                respostaRequisicao.Status = 404;
                respostaRequisicao.Retorno.Add(new MensagemRetorno("EndPoint não encontrado para o nó informado.", true));
                return respostaRequisicao;
            }
            
            Integracao integracaozinha = _repositorioConsulta.Consulta<Integracao>(x => x.Id == endpointzinho.IntegracaoId).FirstOrDefault();
            
            // Montagem da URL
            if (integracaozinha != null && endpointzinho != null)
            {
                string baseURL = integracaozinha.Url?.TrimEnd('/');
                string recurso = endpointzinho.Recurso?.TrimStart('/');
                
                request.Url = $"{baseURL}/{recurso}";
            }

            // Montagem do Header
            if (!string.IsNullOrWhiteSpace(nozinho.Headers))
            {
                Dictionary<string, string> headerDic = _converter.Desserializar<Dictionary<string, string>>(nozinho.Headers);
                try
                {
                    foreach (var item in headerDic)
                    {
                        request.Headers.Add(item.Key, item.Value);
                    }
                }
                catch (JsonException)
                { 
                    respostaRequisicao.Status = 400;
                    respostaRequisicao.Retorno.Add(new MensagemRetorno("Os headers do nó estão em um formato JSON inválido.", true));
                    return respostaRequisicao;
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
            if (!string.IsNullOrWhiteSpace(nozinho.Body))
            {
                request.Body = _converter.Desserializar<object>(nozinho.Body);
                
            }
            
            // Montagem do Verbo
            request.Verbo = endpointzinho.Verbo;

            return _request.Fetch<object>(request);
        }
    }
}
