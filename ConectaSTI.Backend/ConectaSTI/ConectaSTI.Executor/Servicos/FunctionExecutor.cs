using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.Interfaces;
using FGB.Dominio.Interfaces.Utilitarios;
using Jint;

namespace ConectaSTI.Executor.Servicos;

public class FunctionExecutor : IFunctionExecutor
{
    private IConverter _converter;

    public FunctionExecutor(IConverter converter)
    {
        _converter = converter;
    }
    
    public string Executar(Funcao funcao, object dadoAnterior)
    {
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
                ? _converter.Serializar(dadoAnterior)
                : "{}";

            engine.SetValue("rawInputString", jsonInput);

            var scriptFormatado = $@"
                (function() {{
                    const $input = JSON.parse(rawInputString);
                    
                    try {{
                        const resultadoUsuario = (function() {{
                            {funcao.CorpoDaFuncao}
                        }})();

                        return JSON.stringify(resultadoUsuario);
                        
                    }} catch (erroUsuario) {{
                        return JSON.stringify({{ 
                            erroDeCodigo: true, 
                            mensagem: erroUsuario.message 
                        }});
                    }}
                }})();
            ";

            var resultadoJS = engine.Evaluate(scriptFormatado);

            return resultadoJS.AsString();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Falha crítica ao executar a Função JS '{funcao.Nome}' (ID: {funcao.Id}). Detalhes: {ex.Message}"
            );
        }
    }
}