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
    public class StorageExecutor : IStorageExecutor
    {
        private readonly ServicoCrud<Storage> _servico;
        public StorageExecutor(ServicoCrud<Storage> servico)
        {
            _servico = servico;
        }

        public RespostaHttp<object> Salvar(No no)
        {
            // Criação do objeto Storage a partir do No
            Storage storage = new Storage
            {
                Chave = no.ChaveValor,
                Valor = no.Body,
                Validade = no.DataValidade
            };

            // Implementação do método Salvar
            if (_servico.Inclui(storage))
                return new RespostaHttp<object>()
                {
                    Status = 200,
                    Resposta = null,
                    Retorno = new List<MensagemRetorno>()
                    {
                        new MensagemRetorno()
                        {
                            Mensagem = "Valor salvo com sucesso."
                        }
                    }
                };

            return new RespostaHttp<object>()
            {
                Status = 500,
                Resposta = null,
                Retorno = new List<MensagemRetorno>()
                {
                    new MensagemRetorno()
                    {
                        Mensagem = "Erro ao salvar o valor."
                    }
                }
            };
        }

        public RespostaHttp<object> Pegar(string chave)
        {
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

            var storage = _servico.Consulta(x => x.Chave == chave).FirstOrDefault();

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
            
            if(storage.Expirado())
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

            return new RespostaHttp<object>()
            {
                Status = 201,
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
