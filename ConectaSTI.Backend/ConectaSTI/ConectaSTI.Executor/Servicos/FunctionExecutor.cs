using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.Interfaces;
using FGB.Dominio.Interfaces.Utilitarios;
using FGB.Dominio.ObjetoValor;
using FGB.Servicos;
using Jint;
using System;
using System.Diagnostics;
using System.Text.Json;

namespace ConectaSTI.Executor.Servicos;

public class FunctionExecutor : IFunctionExecutor
{
    private const int MaxRecursionDepth = 10;
    private static readonly TimeSpan ScriptTimeout = TimeSpan.FromSeconds(5);
    private const int MaxScriptStatements = 5000;
    private const string EmptyInputJson = "{}";

    private readonly IConverter _converter;

    public FunctionExecutor(IConverter converter)
    {
        _converter = converter;
    }
    
    public RespostaHttp<object> Executar(Funcao funcao, object dadoAnterior)
    {
        var resposta = new RespostaHttp<object>
        {
            Accept = AcceptProxy.Json
        };
        
        var cronometro = Stopwatch.StartNew();

        var engine = new Engine(options =>
        {
            options.LimitRecursion(MaxRecursionDepth)
                .TimeoutInterval(ScriptTimeout)
                .MaxStatements(MaxScriptStatements)
                .Strict();
        });

        try
        {
            if (funcao == null)
            {
                resposta.Status = 404;
                resposta.Retorno.Add(new MensagemRetorno("Função não encontrada.", true));
                return resposta;
            }

            if (string.IsNullOrWhiteSpace(funcao.CorpoDaFuncao))
            {
                resposta.Status = 400;
                resposta.Retorno.Add(new MensagemRetorno("Corpo da função JS não pode ser vazio.", true));
                return resposta;
            }

            string jsonInput = PrepararInputJson(dadoAnterior);

            engine.SetValue("rawInputString", jsonInput);

            var scriptFormatado = $@"
                (function() {{
                    const $input = JSON.parse(rawInputString);
                    
                    const resultadoUsuario = (function() {{
                        {funcao.CorpoDaFuncao}
                    }})();

                    return typeof resultadoUsuario === 'object' 
                        ? JSON.stringify(resultadoUsuario) 
                        : String(resultadoUsuario);
                }})();
            ";

            var resultadoJS = engine.Evaluate(scriptFormatado);

            resposta.Status = 200;
            resposta.RespostaBody = resultadoJS.AsString();
            resposta.Resposta = resposta.RespostaBody;
        }
        catch (Jint.Runtime.JavaScriptException ex)
        {
            resposta.Status = 400;
            resposta.Retorno.Add(new MensagemRetorno($"Erro de execução no código JS: {ex.Error}", true));
        }
        catch (Jint.Runtime.StatementsCountOverflowException)
        {
            resposta.Status = 408;
            resposta.Retorno.Add(new MensagemRetorno("O script excedeu o limite máximo de instruções (Possível loop infinito).", true));
        }
        catch (TimeoutException)
        {
            resposta.Status = 408; 
            resposta.Retorno.Add(new MensagemRetorno("Timeout: O script demorou mais que 5 segundos para rodar.", true));
        }
        catch (Exception ex)
        {
            resposta.Status = 500;
            resposta.Retorno.Add(new MensagemRetorno($"Falha crítica ao executar a Função JS (ID: {funcao?.Id}). Detalhes: {ex.Message}", true));
        }
        finally
        {
            cronometro.Stop();
            resposta.TempoResposta = cronometro.Elapsed;
        }

        return resposta;
    }

    private string PrepararInputJson(object dadoAnterior)
    {
        if (dadoAnterior == null)
            return EmptyInputJson;

        if (dadoAnterior is string dadoString)
            return IsValidJson(dadoString) ? dadoString : _converter.Serializar(dadoString, TipoSerializacao.None);

        return _converter.Serializar(dadoAnterior, TipoSerializacao.None);
    }

    private static bool IsValidJson(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return false;

        try
        {
            using var _ = JsonDocument.Parse(valor);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}