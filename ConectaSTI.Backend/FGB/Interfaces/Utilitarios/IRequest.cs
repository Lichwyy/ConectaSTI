using FGB.Dominio.ObjetoValor;
using System;
using System.Collections.Generic;
using System.Text;

namespace FGB.Dominio.Interfaces.Utilitarios
{
    public interface IRequest
    {
        RespostaHttp<T> Get<T>(RequisicaoHttp request);
        RespostaHttp<T> Post<T>(RequisicaoHttp request);
        RespostaHttp<T> Put<T>(RequisicaoHttp request);
        RespostaHttp<T> Patch<T>(RequisicaoHttp request);
        RespostaHttp<T> Delete<T>(RequisicaoHttp request);
        RespostaHttp<T> Fetch<T>(RequisicaoHttp request);
        Task<RespostaHttp<T>> GetAsync<T>(RequisicaoHttp request);
        Task<RespostaHttp<T>> PostAsync<T>(RequisicaoHttp request);
        Task<RespostaHttp<T>> PutAsync<T>(RequisicaoHttp request);
        Task<RespostaHttp<T>> PatchAsync<T>(RequisicaoHttp request);
        Task<RespostaHttp<T>> DeleteAsync<T>(RequisicaoHttp request);
        Task<RespostaHttp<T>> FetchAsync<T>(RequisicaoHttp request);
    }
}
