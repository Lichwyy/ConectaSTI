using FGB.Dominio.Interfaces.Utilitarios;
using FGB.Dominio.ObjetoValor;
using FGB.Servicos;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FGB.Dominio.Servicos
{
    public class ServicoRequest : IRequest
    {
        public RespostaHttp<T> Get<T>(RequisicaoHttp request)
        {
            request.Verbo = VerboHttp.GET;
            return Fetch<T>(request);
        }

        public RespostaHttp<T> Post<T>(RequisicaoHttp request)
        {
            request.Verbo = VerboHttp.POST;
            return Fetch<T>(request);
        }

        public RespostaHttp<T> Put<T>(RequisicaoHttp request)
        {
            request.Verbo = VerboHttp.PUT;
            return Fetch<T>(request);
        }

        public RespostaHttp<T> Patch<T>(RequisicaoHttp request)
        {
            request.Verbo = VerboHttp.PATCH;
            return Fetch<T>(request);
        }

        public RespostaHttp<T> Delete<T>(RequisicaoHttp request)
        {
            request.Verbo = VerboHttp.DELETE;
            return Fetch<T>(request);
        }

        public RespostaHttp<T> Fetch<T>(RequisicaoHttp request)
        {
            return FetchAsync<T>(request).GetAwaiter().GetResult();
        }

        public Task<RespostaHttp<T>> GetAsync<T>(RequisicaoHttp request)
        {
            request.Verbo = VerboHttp.GET;
            return FetchAsync<T>(request);
        }

        public Task<RespostaHttp<T>> PostAsync<T>(RequisicaoHttp request)
        {
            request.Verbo = VerboHttp.POST;
            return FetchAsync<T>(request);
        }

        public Task<RespostaHttp<T>> PutAsync<T>(RequisicaoHttp request)
        {
            request.Verbo = VerboHttp.PUT;
            return FetchAsync<T>(request);
        }

        public Task<RespostaHttp<T>> PatchAsync<T>(RequisicaoHttp request)
        {
            request.Verbo = VerboHttp.PATCH;
            return FetchAsync<T>(request);
        }

        public Task<RespostaHttp<T>> DeleteAsync<T>(RequisicaoHttp request)
        {
            request.Verbo = VerboHttp.DELETE;
            return FetchAsync<T>(request);
        }

        public async Task<RespostaHttp<T>> FetchAsync<T>(RequisicaoHttp request)
        {
            var resposta = new RespostaHttp<T>
            {
                Accept = request?.Accept ?? AcceptProxy.Json
            };

            if (request == null || string.IsNullOrWhiteSpace(request.UrlRequest))
            {
                resposta.Retorno.Add(new MensagemRetorno("Requisição inválida. Informe Url/Query.", true));
                return resposta;
            }

            var inicio = DateTime.UtcNow;

            try
            {
                using var handler = new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };

                if (request.IgnorarCertificadoSsl)
                {
                    handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                }

                using var client = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromSeconds(request.TimeoutSegundos > 0 ? request.TimeoutSegundos : 30)
                };

                using var requisicao = MontaRequisicao(request);
                using var response = await client.SendAsync(requisicao).ConfigureAwait(false);

                resposta.Status = (int)response.StatusCode;
                resposta.RespostaBody = response.Content == null
                    ? null
                    : await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    if (!string.IsNullOrWhiteSpace(resposta.RespostaBody))
                    {
                        resposta.Resposta = Desserializar<T>(resposta.Accept, resposta.RespostaBody);
                    }
                }
                else
                {
                    ProcessarMensagensErro(response, resposta);
                }
            }
            catch (TaskCanceledException)
            {
                resposta.Retorno.Add(new MensagemRetorno("Timeout na requisição.", true));
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
