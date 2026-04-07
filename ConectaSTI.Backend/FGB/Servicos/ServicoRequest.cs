using FGB.Dominio.Interfaces.Utilitarios;
using FGB.Dominio.ObjetoValor;
using FGB.Servicos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FGB.Dominio.Servicos
{
    public class ServicoRequest : IRequest
    {
        private static readonly HttpClient _httpClientPadrao = CriarHttpClient(false);
        private static readonly HttpClient _httpClientInseguro = CriarHttpClient(true);

        private readonly ServicoRequestOptions _options;
        private readonly ILogger<ServicoRequest> _logger;

        public ServicoRequest(
            IOptions<ServicoRequestOptions> options,
            ILogger<ServicoRequest> logger)
        {
            _options = options?.Value ?? new ServicoRequestOptions();
            _logger = logger;
        }

        public RespostaHttp<T> Get<T>(RequisicaoHttp request)
        {
            return Fetch<T>(PrepararRequest(request, VerboHttp.GET));
        }

        public RespostaHttp<T> Post<T>(RequisicaoHttp request)
        {
            return Fetch<T>(PrepararRequest(request, VerboHttp.POST));
        }

        public RespostaHttp<T> Put<T>(RequisicaoHttp request)
        {
            return Fetch<T>(PrepararRequest(request, VerboHttp.PUT));
        }

        public RespostaHttp<T> Patch<T>(RequisicaoHttp request)
        {
            return Fetch<T>(PrepararRequest(request, VerboHttp.PATCH));
        }

        public RespostaHttp<T> Delete<T>(RequisicaoHttp request)
        {
            return Fetch<T>(PrepararRequest(request, VerboHttp.DELETE));
        }

        public RespostaHttp<T> Fetch<T>(RequisicaoHttp request)
        {
            var resposta = CriarResposta<T>(request);
            if (!ValidarRequest(request, resposta))
            {
                return resposta;
            }

            var inicio = DateTime.UtcNow;
            using var timeoutCts = new CancellationTokenSource(ObterTimeout(request));

            try
            {
                var client = CriarClient(request);
                using var requisicao = MontaRequisicao(request);
                using var response = client.Send(requisicao, timeoutCts.Token);

                resposta.Status = (int)response.StatusCode;
                resposta.RespostaBody = LerConteudoSincrono(response.Content);

                ProcessarResposta(response, resposta);
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
            {
                resposta.Retorno.Add(new MensagemRetorno("Timeout na requisição.", true));
            }
            catch (OperationCanceledException ex)
            {
                resposta.Retorno.Add(new MensagemRetorno(ex));
            }
            catch (HttpRequestException ex)
            {
                resposta.Retorno.Add(new MensagemRetorno(ex));
            }
            catch (Exception ex)
            {
                resposta.Retorno.Add(new MensagemRetorno(ex));
            }
            finally
            {
                resposta.TempoResposta = DateTime.UtcNow - inicio;
            }

            return resposta;
        }

        public Task<RespostaHttp<T>> GetAsync<T>(RequisicaoHttp request)
        {
            return FetchAsync<T>(PrepararRequest(request, VerboHttp.GET));
        }

        public Task<RespostaHttp<T>> PostAsync<T>(RequisicaoHttp request)
        {
            return FetchAsync<T>(PrepararRequest(request, VerboHttp.POST));
        }

        public Task<RespostaHttp<T>> PutAsync<T>(RequisicaoHttp request)
        {
            return FetchAsync<T>(PrepararRequest(request, VerboHttp.PUT));
        }

        public Task<RespostaHttp<T>> PatchAsync<T>(RequisicaoHttp request)
        {
            return FetchAsync<T>(PrepararRequest(request, VerboHttp.PATCH));
        }

        public Task<RespostaHttp<T>> DeleteAsync<T>(RequisicaoHttp request)
        {
            return FetchAsync<T>(PrepararRequest(request, VerboHttp.DELETE));
        }

        public async Task<RespostaHttp<T>> FetchAsync<T>(RequisicaoHttp request)
        {
            var resposta = CriarResposta<T>(request);

            if (!ValidarRequest(request, resposta))
            {
                return resposta;
            }

            var inicio = DateTime.UtcNow;
            using var timeoutCts = new CancellationTokenSource(ObterTimeout(request));

            try
            {
                var client = CriarClient(request);

                using var requisicao = MontaRequisicao(request);
                using var response = await client.SendAsync(requisicao, timeoutCts.Token).ConfigureAwait(false);

                resposta.Status = (int)response.StatusCode;
                resposta.RespostaBody = response.Content == null
                    ? null
                    : await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                ProcessarResposta(response, resposta);
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
            {
                resposta.Retorno.Add(new MensagemRetorno("Timeout na requisição.", true));
            }
            catch (OperationCanceledException ex)
            {
                resposta.Retorno.Add(new MensagemRetorno(ex));
            }
            catch (HttpRequestException ex)
            {
                resposta.Retorno.Add(new MensagemRetorno(ex));
            }
            catch (Exception ex)
            {
                resposta.Retorno.Add(new MensagemRetorno(ex));
            }
            finally
            {
                resposta.TempoResposta = DateTime.UtcNow - inicio;
            }

            return resposta;
        }

        private static RequisicaoHttp PrepararRequest(RequisicaoHttp request, VerboHttp verbo)
        {
            if (request != null)
            {
                request.Verbo = verbo;
            }

            return request;
        }

        private static RespostaHttp<T> CriarResposta<T>(RequisicaoHttp request)
        {
            return new RespostaHttp<T>
            {
                Accept = request?.Accept ?? AcceptProxy.Json
            };
        }

        private static bool ValidarRequest<T>(RequisicaoHttp request, RespostaHttp<T> resposta)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.UrlRequest))
            {
                resposta.Retorno.Add(new MensagemRetorno("Requisição inválida. Informe Url/Query.", true));
                return false;
            }

            return true;
        }

        private HttpClient CriarClient(RequisicaoHttp request)
        {
            var usarClientInseguro = DeveUsarClientInseguro(request);
            return usarClientInseguro ? _httpClientInseguro : _httpClientPadrao;
        }

        private bool DeveUsarClientInseguro(RequisicaoHttp request)
        {
            if (!request.IgnorarCertificadoSsl)
            {
                return false;
            }

            if (_options.PermitirIgnorarCertificadoSsl)
            {
                return true;
            }

            _logger.LogWarning("IgnorarCertificadoSsl bloqueado fora de ambiente controlado para URL {Url}", request.UrlRequest);
            return false;
        }

        private static HttpClient CriarHttpClient(bool ignorarCertificadoSsl)
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            if (ignorarCertificadoSsl)
            {
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            return new HttpClient(handler)
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
        }

        private static TimeSpan ObterTimeout(RequisicaoHttp request)
        {
            return TimeSpan.FromSeconds(request.TimeoutSegundos > 0 ? request.TimeoutSegundos : 30);
        }

        private HttpRequestMessage MontaRequisicao(RequisicaoHttp requisicao)
        {
            var metodo = new HttpMethod(requisicao.Verbo.ToString());
            var request = new HttpRequestMessage(metodo, requisicao.UrlRequest);

            if (metodo != HttpMethod.Get && metodo != HttpMethod.Delete)
            {
                var raw = ObterBodyRaw(requisicao);
                if (!string.IsNullOrWhiteSpace(raw))
                {
                    requisicao.BodyRaw = raw;
                    request.Content = new StringContent(raw, Encoding.UTF8, GetContentType(requisicao.ContentType));
                }
            }

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(GetContentType(requisicao.Accept)));
            AdicionarHeaders(request, requisicao);
            return request;
        }

        private static string LerConteudoSincrono(HttpContent content)
        {
            if (content == null)
            {
                return null;
            }

            using var stream = content.ReadAsStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, true);
            return reader.ReadToEnd();
        }

        private void ProcessarResposta<T>(HttpResponseMessage response, RespostaHttp<T> resposta)
        {
            if (response.IsSuccessStatusCode)
            {
                if (!string.IsNullOrWhiteSpace(resposta.RespostaBody))
                {
                    resposta.Resposta = Desserializar<T>(resposta.Accept, resposta.RespostaBody);
                }

                return;
            }

            ProcessarMensagensErro(response, resposta);
        }

        private static void AdicionarHeaders(HttpRequestMessage destino, RequisicaoHttp origem)
        {
            if (origem.Headers == null)
            {
                return;
            }

            foreach (var key in origem.Headers.AllKeys)
            {
                var valor = origem.Headers[key];
                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(valor))
                {
                    continue;
                }

                if (string.Equals(key, "Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    if (destino.Content != null)
                    {
                        destino.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(valor);
                    }

                    continue;
                }

                if (string.Equals(key, "Accept", StringComparison.OrdinalIgnoreCase))
                {
                    destino.Headers.Accept.Clear();
                    destino.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(valor));
                    continue;
                }

                if (!destino.Headers.TryAddWithoutValidation(key, valor) && destino.Content != null)
                {
                    destino.Content.Headers.TryAddWithoutValidation(key, valor);
                }
            }
        }

        private void ProcessarMensagensErro<T>(HttpResponseMessage response, RespostaHttp<T> resposta)
        {
            if (response.StatusCode == (HttpStatusCode)422 && resposta.Accept != AcceptProxy.Text)
            {
                var mensagens = Desserializar<List<MensagemRetorno>>(resposta.Accept, resposta.RespostaBody);
                if (mensagens != null && mensagens.Count > 0)
                {
                    resposta.Retorno = mensagens;
                    return;
                }
            }

            var mensagem = string.IsNullOrWhiteSpace(resposta.RespostaBody)
                ? $"Erro HTTP {(int)response.StatusCode} - {response.ReasonPhrase}"
                : resposta.RespostaBody;

            resposta.Retorno.Add(new MensagemRetorno(mensagem, true));
        }

        public T Desserializar<T>(AcceptProxy tipo, string sr)
        {
            if (string.IsNullOrWhiteSpace(sr))
            {
                return default;
            }

            switch (tipo)
            {
                case AcceptProxy.Json:
                    return JsonSerializer.Deserialize<T>(sr, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                case AcceptProxy.Xml:
                case AcceptProxy.Text:
                case AcceptProxy.Html:
                case AcceptProxy.FormUrlEncoded:
                    if (typeof(T) == typeof(string) || typeof(T) == typeof(object))
                    {
                        return (T)(object)sr;
                    }

                    return default;
                default:
                    return default;
            }
        }

        private static string ObterBodyRaw(RequisicaoHttp request)
        {
            if (!string.IsNullOrWhiteSpace(request.BodyRaw))
            {
                return request.BodyRaw;
            }

            if (request.Body == null)
            {
                return null;
            }

            return request.ContentType == AcceptProxy.Json
                ? JsonSerializer.Serialize(request.Body)
                : request.Body.ToString();
        }

        private static string GetContentType(AcceptProxy tipo)
        {
            return tipo switch
            {
                AcceptProxy.Json => "application/json",
                AcceptProxy.Xml => "application/xml",
                AcceptProxy.Html => "text/html",
                AcceptProxy.Text => "text/plain",
                AcceptProxy.FormUrlEncoded => "application/x-www-form-urlencoded",
                _ => "application/json"
            };
        }
    }
}
