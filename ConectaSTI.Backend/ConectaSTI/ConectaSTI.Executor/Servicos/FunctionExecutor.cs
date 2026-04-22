using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.Interfaces;
using FGB.Dominio.Interfaces.Utilitarios;
using FGB.Dominio.ObjetoValor; // Referência para a sua RespostaHttp e MensagemRetorno
using FGB.Servicos;
using Jint;
using System;
using System.Diagnostics;

namespace ConectaSTI.Executor.Servicos;

public class FunctionExecutor : IFunctionExecutor
{
    private readonly IConverter _converter;

    public FunctionExecutor(IConverter converter)
    {
        _converter = converter;
    }
    
    public RespostaHttp<string> Executar(Funcao funcao, object dadoAnterior)
    {
        var resposta = new RespostaHttp<string>
        {
            Accept = AcceptProxy.Json
        };
        
        var cronometro = Stopwatch.StartNew();

        var engine = new Engine(options =>
        {
            options.LimitRecursion(10)
                .TimeoutInterval(TimeSpan.FromSeconds(5))
                .MaxStatements(5000)
                .Strict();
        });

        try
        {
            string jsonInput = dadoAnterior != null
                ? _converter.Serializar(dadoAnterior, TipoSerializacao.None)
                : "{}";

            engine.SetValue("rawInputString", jsonInput);

            var scriptFormatado = $@"
                (function() {{
                    const $input = JSON.parse(rawInputString);
                    
                    const resultadoUsuario = (function() {{
                        {funcao.CorpoDaFuncao}
                    }})();

                    // Se for um objeto, devolve JSON, se for primitivo, devolve string
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
}