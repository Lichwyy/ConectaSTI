using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using ConectaSTI.Dominio.DTOs;
using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.Interfaces;
using FGB.IRepositorios;

namespace ConectaSTI.Executor.Servicos;

public class VersionarExecutor : IVersionarExecutor
{
    private readonly IRepositorioSessao _repositorioSessao;
    private readonly IRepositorioConsulta _consulta;

    public VersionarExecutor(IRepositorioSessao repositorioSessao, IRepositorioConsulta consulta)
    {
        _repositorioSessao = repositorioSessao;
        _consulta = consulta;
    }

    public FluxoVersionado Execute(long fluxoId)
    {
        using (var trans = _repositorioSessao.IniciaTransacao())
        {
            try
            {
                Fluxo fluxo = _repositorioSessao.RetornaComLock<Fluxo>(fluxoId);

                int ultimaVersao = _consulta.Consulta<FluxoVersionado>(x => x.FluxoId == fluxoId)
                    .Select(x => (int?)x.Versao)
                    .Max() ?? 0;

                int proximaVersao = ultimaVersao + 1;

                FluxoVersionado fluxoVersionado = new FluxoVersionado()
                {
                    FluxoId = fluxoId,
                    Nome = fluxo.Nome,
                    Versao = proximaVersao,
                };

                var opcoes = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = false
                };

                FluxoDTO fluxoDto = new FluxoDTO()
                {
                    Operacoes = GetAllOperation(fluxo)
                };

                string fluxoSerializado = JsonSerializer.Serialize(fluxoDto, opcoes);

                fluxoVersionado.Payload = fluxoSerializado;

                // --- INÍCIO DA SOLUÇÃO 1 CORRIGIDA ---
                fluxoVersionado.CriadoEm = DateTime.Now;
                fluxoVersionado.UltimaAlteracao = DateTime.Now;

                // 1. Obtém o repositório de CRUD a partir da sessão
                var repositorio = _repositorioSessao.GetRepositorio();

                // 2. Faz o inclui utilizando o repositório correto
                repositorio.Inclui(fluxoVersionado);

                // 3. O Executor comita a transação da sessão principal
                _repositorioSessao.CommitaTransacao();
                // --- FIM DA SOLUÇÃO 1 CORRIGIDA ---

                return fluxoVersionado;
            }
            catch (Exception)
            {
                try
                {
                    _repositorioSessao.RollBackTransacao();
                }
                catch { }

                throw;
            }
        }
    }

    private List<OperacaoDTO> GetAllOperation(Fluxo fluxo)
    {
        List<OperacaoDTO> listaOperacao = new List<OperacaoDTO>();

        foreach (Operacao operacao in fluxo.Operacoes)
        {
            OperacaoDTO operacaoDto = new OperacaoDTO()
            {
                BackoffDelay = operacao.BackoffDelay,
                BackoffMultiplier = operacao.BackoffMultiplier,
                BackoffType = operacao.BackoffType,
                Erro = operacao.Erro,
                MaximoRepeticao = operacao.MaximoRepeticao,
                NoId = operacao.NoId,
                Ordem = operacao.Ordem,
                Repetir = operacao.Repetir,
                Timeout = operacao.Timeout,
                UsarDadosAnterior = operacao.UsarDadosAnterior
            };

            listaOperacao.Add(operacaoDto);
        }

        return listaOperacao;
    }
}