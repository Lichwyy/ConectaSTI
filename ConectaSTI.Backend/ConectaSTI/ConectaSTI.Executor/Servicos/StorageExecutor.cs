using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.Interfaces;
using ConectaSTI.Dominio.ObjetosValor;
using ConectaSTI.Dominio.Servicos;
using FGB.Dominio.ObjetoValor;
using FGB.IRepositorios;
using FGB.Servicos;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConectaSTI.Executor.Servicos
{
    public class StorageExecutor : IStorageExecutor
    {
        private readonly ServicoStorage _servico;
        public StorageExecutor(ServicoStorage servico)
        {
            _servico = servico;
        }

        public RespostaHttp<object> Salvar(No no)
        {
            // Validando o No de entrada
            if (no == null)
            {
                return new RespostaHttp<object>()
                {
                    Status = 400,
                    Resposta = null,
                    Retorno = new List<MensagemRetorno>()
                    {
                        new MensagemRetorno()
                        {
                            Mensagem = "O No não pode ser nulo."
                        }
                    }
                };
            }

            // Criação do objeto Storage a partir do No
            Storage storage = new Storage
            {
                Chave = no.ChaveValor,
                Valor = no.Body,
                Validade = DateTime.Now.AddMinutes(no.TempoMinutoValidade)
            };

            // Salvando o Storage usando o serviço de CRUD
            if (_servico.Inclui(storage))
                return new RespostaHttp<object>()
                {
                    Status = 201,
                    Resposta = no.Body,
                    Retorno = new List<MensagemRetorno>() { new MensagemRetorno() { Mensagem = "Valor salvo com sucesso." } }
                };

            // Em caso de falha ao salvar, retornamos um erro.
            return new RespostaHttp<object>()
            {
                Status = 500,
                Resposta = null,
                Retorno = new List<MensagemRetorno>()
                {
                    new MensagemRetorno()
                    {
                        Mensagem = _servico.Mensagens.FirstOrDefault()?.Mensagem ?? "Erro ao salvar o valor."
                    }
                }
            };
        }

        public RespostaHttp<object> Pegar(string chave)
        {
            // Validando a chave de entrada
            if (String.IsNullOrWhiteSpace(chave))
            {
                return new RespostaHttp<object>()
                {
                    Status = 400,
                    Resposta = null,
                    Retorno = new List<MensagemRetorno>()
                    {
                        new MensagemRetorno()
                        {
                            Mensagem = "Chave não pode ser nula ou vazia."
                        }
                    }
                };
            }

            // Consultando o Storage usando a chave fornecida
            var storage = _servico.Consulta(x => x.Chave == chave).FirstOrDefault();

            // Verificando se o Storage foi encontrado
            if (storage == null)
            {
                return new RespostaHttp<object>()
                {
                    Status = 404,
                    Resposta = null,
                    Retorno = new List<MensagemRetorno>()
                    {
                        new MensagemRetorno()
                        {
                            Mensagem = "Valor não encontrado para a chave fornecida."
                        }
                    }
                };
            }

            // Verificando se o valor armazenado expirou
            if (storage.Expirado())
            {
                return new RespostaHttp<object>()
                {
                    Status = 410,
                    Resposta = null,
                    Retorno = new List<MensagemRetorno>()
                    {
                        new MensagemRetorno()
                        {
                            Mensagem = "Valor expirado para a chave fornecida."
                        }
                    }
                };
            }

            // Retornando o valor armazenado com sucesso
            return new RespostaHttp<object>()
            {
                Status = 200,
                Resposta = storage.Valor,
                Retorno = new List<MensagemRetorno>()
                {
                    new MensagemRetorno()
                    {
                        Mensagem = "Valor recuperado com sucesso."
                    }
                }
            };
        }
    }
}
