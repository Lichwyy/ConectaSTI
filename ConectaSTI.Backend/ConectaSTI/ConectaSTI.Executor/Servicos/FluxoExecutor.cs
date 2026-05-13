using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.Interfaces;
using ConectaSTI.Dominio.ObjetosValor;
using FGB.Dominio.ObjetoValor;
using FGB.IRepositorios;
using FGB.Servicos;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConectaSTI.Executor.Servicos
{
    public class FluxoExecutor : IFluxoExecutor
    {

        private readonly IRepositorioConsulta _repositorioConsulta;
        private readonly IRequestExecutor _requestExecutor;
        private readonly IFunctionExecutor _functionExecutor;
        private readonly IStorageExecutor _storageExecutor;

        public FluxoExecutor(IRepositorioConsulta repositorioConsulta, IRequestExecutor requestExecutor, IFunctionExecutor functionExecutor, IStorageExecutor storageExecutor)
        {
            _repositorioConsulta = repositorioConsulta;
            _requestExecutor = requestExecutor;
            _functionExecutor = functionExecutor;
            _storageExecutor = storageExecutor;
        }

        public async Task<RespostaHttp<object>> Executar(long fluxoId)
        {
            // Criando uma resposta HTTP genérica para retornar o resultado da execução do fluxo
            RespostaHttp<object> resposta = new RespostaHttp<object>();

            // Consultando o fluxo no banco de dados usando o repositório de consulta
            Fluxo fluxo = _repositorioConsulta.Consulta<Fluxo>(x => x.Id == fluxoId).FirstOrDefault();

            // Verificando se o fluxo foi encontrado
            if (fluxo == null)
            {
                resposta.Status = 404;
                resposta.Retorno.Add(new MensagemRetorno($"Nao foi possivel achar o fluxo com id {fluxoId}", true));
                return resposta;
            }

            // Verificando se o fluxo possui operações associadas, pois um fluxo sem operações não pode ser executado
            if (fluxo.Operacoes == null || !fluxo.Operacoes.Any())
            {
                resposta.Status = 400;
                resposta.Retorno.Add(new MensagemRetorno("Fluxo nao possui operacoes.", true));
                return resposta;
            }

            // Ordenando as operações do fluxo com base na propriedade Ordem para garantir que sejam executadas na sequência correta
            var operacoesOrdenadas = fluxo.Operacoes.OrderBy(x => x.Ordem).ToList();

            // Pré-carregando todos os nós necessários para evitar consultas repetidas ao banco dentro do foreach
            var noIds = operacoesOrdenadas.Select(x => x.NoId).Distinct().ToList();
            var nosPorId = _repositorioConsulta
                .Consulta<No>(x => noIds.Contains(x.Id))
                .ToDictionary(x => x.Id, x => x);
            
            // Variável para armazenar o resultado da operação anterior, que pode ser usada como entrada para a próxima operação no fluxo
            object dadoAnterior = null;

            // Iterando sobre as operações do fluxo e executando cada uma delas
            foreach (Operacao operacao in operacoesOrdenadas)
            {
                // Obtendo o nó associado à operação a partir dos dados pré-carregados
                if (!nosPorId.TryGetValue(operacao.NoId, out No no))
                {
                    resposta.Status = 404;
                    resposta.Retorno.Add(new MensagemRetorno($"Nao foi possivel achar o no com id {operacao.NoId} para a operacao com id {operacao.Id}", true));
                    return resposta;
                }

                // Variáveis para controle de tentativas e sucesso da execução do nó, que podem ser usadas para implementar políticas de repetição em caso de falhas
                int tentativas = 0;
                bool sucesso = false;
                bool deveTentarNovamente;

                // Loop para executar o nó, que pode ser repetido de acordo com a política de repetição definida na operação em caso de falhas
                do
                {
                    tentativas++;

                    // Executando o nó com base no seu tipo (Requisição, FunçãoJS, etc.) e atualizando a resposta HTTP de acordo
                    switch (no.Tipo)
                    {
                        // Se o tipo do nó for Requisição, usar o executor de requisições para enviar a requisição e obter a resposta
                        case TipoNo.Requisicao:
                            if (operacao.UsarDadosAnterior)
                            {
                                no.Body = dadoAnterior?.ToString();
                            }
                            resposta = _requestExecutor.EnviarRequisicao(no);
                            break;

                        // Se o tipo do nó for FuncaoJS, usar o executor de funções para executar a função JavaScript e obter a resposta
                        case TipoNo.FuncaoJS:
                            // Consultando a função associada ao nó no banco de dados
                            Funcao funcao = _repositorioConsulta.Consulta<Funcao>(x => x.Id == no.FuncaoId).FirstOrDefault();

                            // Verificando se a função foi encontrada
                            if (funcao == null)
                            {
                                resposta.Status = 404;
                                resposta.Retorno.Add(new MensagemRetorno($"Nao foi possivel achar a funcao com id {no.FuncaoId} para o no com id {no.Id}", true));
                                return resposta;
                            }

                            // Executando a função JavaScript usando o executor de funções e atualizando a resposta HTTP com o resultado da execução
                            resposta = _functionExecutor.Executar(funcao, dadoAnterior);
                            break;

                        // Se o tipo do nó for SalvarStorage, usar o executor de storage para salvar o valor no storage e obter a resposta
                        case TipoNo.SalvarStorage:
                            no.Body = dadoAnterior?.ToString();
                            resposta = _storageExecutor.Salvar(no);
                            break;

                        // Se o tipo do nó for PegarStorage, usar o executor de storage para pegar o valor do storage e obter a resposta
                        case TipoNo.PegarStorage:
                            resposta = _storageExecutor.Pegar(no.ChaveValor);
                            break;
                    }

                    // Verificando se a execução do nó foi bem-sucedida com base no status da resposta HTTP (códigos 2xx indicam sucesso)
                    sucesso = resposta.Status >= 200 && resposta.Status < 300;

                    // Se a execução do nó não foi bem-sucedida e a operação tem a política de repetição habilitada
                    // aguardar um tempo definido pela política de backoff antes de tentar novamente, incrementando o número de tentativas
                    deveTentarNovamente = !sucesso && operacao.Repetir && tentativas <= operacao.MaximoRepeticao;

                    if (deveTentarNovamente)
                    {
                        int delay = operacao.BackoffDelay * (int)Math.Pow(operacao.BackoffMultiplier, tentativas - 1);

                        await Task.Delay(delay);
                    }
                } while (deveTentarNovamente);

                // Se a execução do nó não foi bem-sucedida após as tentativas de repetição
                // verificar o tipo de erro definido na operação para decidir se deve falhar o fluxo, continuar para a próxima operação ou executar uma compensação
                if (!sucesso)
                {
                    switch (operacao.Erro)
                    {
                        case TipoErro.FalharFluxo:
                            return resposta;

                        case TipoErro.ContinuarFluxo:
                            break;

                        // Ainda tem que adicionar compensao, perguntar para Brandi
                    }
                }

                //Atualizando a variável dadoAnterior com o resultado da execução do nó, que pode ser usada como entrada para a próxima operação no fluxo
                dadoAnterior = resposta.Resposta;
            }

            // Se todas as operações do fluxo foram executadas com sucesso, atualizar a resposta HTTP para indicar o sucesso da execução do fluxo
            return resposta;
        }
    }
}
