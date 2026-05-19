using FGB.Dominio.Entidades;
using FGB.Dominio.Extensoes;
using FGB.Dominio.Interfaces.Seguranca;
using FGB.Entidades;
using FGB.IRepositorios;
using System.Globalization;
using System.Reflection;

namespace FGB.Servicos
{
    public class ServicoCrud<T> : ServicoConsulta<T> where T : EntidadeBase
    {
        protected ICurrentUserContext CurrentUserContext { get; }

        public event MergeHandler<T> PreMerge;
        public event MergeHandler<T> PosMerge;
        public event ExcluiHandler<T> PreExclui;
        public event ExcluiHandler<T> PosExclui;
        public event IncluiHandler<T> PreInclui;
        public event IncluiHandler<T> PosInclui;

        public ServicoCrud(IRepositorioSessao repositorio, ICurrentUserContext currentUserContext) : base(repositorio)
        {
            CurrentUserContext = currentUserContext;
        }

        public virtual bool Validacoes(T entidade)
        {
            Mensagens.Clear();
            if (entidade == null)
            {
                Mensagens.Add("Entidade vazia na requisição.");
                return false;
            }

            return Valida(entidade);
        }

        public virtual bool Valida(T entidade)
        {
            return !Mensagens.HasErro();
        }

        public bool MakeCrudTransaction(Action<IRepositorio> transaction)
        {
            using (Repositorio.IniciaTransacao())
            {
                var repositorio = Repositorio.GetRepositorio();
                try
                {
                    transaction(repositorio);
                    Repositorio.CommitaTransacao();
                }
                catch (Exception ex)
                {
                    Repositorio.RollBackTransacao();
                    Mensagens.Add(ex.Message, true);
                }

                return !Mensagens.HasErro();
            }
        }

        public async Task<bool> MakeCrudTransactionAsync(Func<IRepositorio, Task> transaction)
        {
            using (Repositorio.IniciaTransacao())
            {
                var repositorio = Repositorio.GetRepositorio();
                try
                {
                    await transaction(repositorio);
                    await Repositorio.CommitaTransacaoAsync();
                }
                catch (Exception ex)
                {
                    await Repositorio.RollBackTransacaoAsync();
                    Mensagens.Add(ex.Message, true);
                }

                return !Mensagens.HasErro();
            }
        }

        public virtual bool Inclui(params T[] entidades)
        {
            return ProcessoInclusao(entidades, () => MakeCrudTransaction(repo =>
            {
                foreach (var entidade in entidades)
                {
                    entidade.VincularColecoes();
                    entidade.CriadoEm = entidade.CriadoEm ?? DateTime.Now;
                    entidade.UltimaAlteracao = DateTime.Now;
                    repo.Inclui(entidade);
                    RegistrarAuditoria(repo, null, entidade, "criar");
                }
            }));
        }

        public virtual async Task<bool> IncluiAsync(params T[] entidades)
        {
            return await ProcessoInclusaoAsync(entidades, async () => await MakeCrudTransactionAsync(async repo =>
            {
                foreach (var entidade in entidades)
                {
                    entidade.VincularColecoes();
                    entidade.CriadoEm = DateTime.Now;
                    entidade.UltimaAlteracao = DateTime.Now;
                    await repo.IncluiAsync(entidade);
                    await RegistrarAuditoriaAsync(repo, null, entidade, "criar");
                }
            }));
        }

        public virtual bool ProcessoInclusao(T[] entidades, Func<bool> process)
        {
            if (entidades.Length == 0)
                return true;

            if (!entidades.All(Validacoes))
                return false;

            foreach (var entidade in entidades)
            {
                entidade.CriadoEm = DateTime.Now;
                entidade.UltimaAlteracao = DateTime.Now;
                try { PreInclui?.Invoke(new IncluiInfo<T>(entidade)); } catch (Exception ex) { Mensagens.Add(ex.Message); }
            }

            var sucesso = process();
            if (sucesso)
            {
                foreach (var entidade in entidades)
                {
                    try { PosInclui?.Invoke(new IncluiInfo<T>(entidade)); } catch (Exception ex) { Mensagens.Add(ex.Message); }
                }
            }

            return sucesso;
        }

        public virtual async Task<bool> ProcessoInclusaoAsync(T[] entidades, Func<Task<bool>> process)
        {
            if (entidades.Length == 0)
                return true;

            if (!entidades.All(Validacoes))
                return false;

            foreach (var entidade in entidades)
            {
                entidade.CriadoEm = DateTime.Now;
                entidade.UltimaAlteracao = DateTime.Now;
                try { PreInclui?.Invoke(new IncluiInfo<T>(entidade)); } catch (Exception ex) { Mensagens.Add(ex.Message); }
            }

            var sucesso = await process();
            if (sucesso)
            {
                foreach (var entidade in entidades)
                {
                    try { PosInclui?.Invoke(new IncluiInfo<T>(entidade)); } catch (Exception ex) { Mensagens.Add(ex.Message); }
                }
            }

            return sucesso;
        }

        public virtual T Merge(T entidade)
        {
            if (!ProcessoMerge(entidade, entidadeOld => MakeCrudTransaction(repo =>
            {
                entidade.VincularColecoes();
                entidade.UltimaAlteracao = DateTime.Now;
                entidade.CriadoEm = entidadeOld.CriadoEm;
                RegistrarAuditoria(repo, entidadeOld, entidade, "atualizar");
                entidade = repo.Merge(entidade);
            })))
            {
                return null;
            }

            return entidade;
        }

        public virtual async Task<T> MergeAsync(T entidade)
        {
            return await ProcessoMergeAsync(entidade, async entidadeOld => await MakeCrudTransactionAsync(async repo =>
            {
                entidade.VincularColecoes();
                entidade.UltimaAlteracao = DateTime.Now;
                entidade.CriadoEm = entidadeOld.CriadoEm;
                await RegistrarAuditoriaAsync(repo, entidadeOld, entidade, "atualizar");
                entidade = await repo.MergeAsync(entidade);
            })) ? entidade : null;
        }

        protected virtual bool ProcessoMerge(T entidade, Func<T, bool> process)
        {
            if (!Validacoes(entidade))
                return false;

            var entidadeOld = Retorna(entidade.Id);
            if (entidadeOld == null)
                return false;

            try { PreMerge?.Invoke(new MergeInfo<T>(entidadeOld, entidade)); } catch (Exception ex) { Mensagens.Add(ex.Message); }

            var sucesso = process(entidadeOld);
            if (sucesso)
            {
                try { PosMerge?.Invoke(new MergeInfo<T>(entidadeOld, entidade)); } catch (Exception ex) { Mensagens.Add(ex.Message); }
            }

            return sucesso;
        }

        protected virtual async Task<bool> ProcessoMergeAsync(T entidade, Func<T, Task<bool>> process)
        {
            if (!Validacoes(entidade))
                return false;

            var entidadeOld = await RetornaAsync(entidade.Id);
            if (entidadeOld == null)
                return false;

            try { PreMerge?.Invoke(new MergeInfo<T>(entidadeOld, entidade)); } catch (Exception ex) { Mensagens.Add(ex.Message); }

            var sucesso = await process(entidadeOld);
            if (sucesso)
            {
                try { PosMerge?.Invoke(new MergeInfo<T>(entidadeOld, entidade)); } catch (Exception ex) { Mensagens.Add(ex.Message); }
            }

            return sucesso;
        }

        public virtual T Exclui(long id)
        {
            var entidade = Retorna(id);
            if (entidade == null)
            {
                Mensagens.Add("Registro não encontrado.");
                return null;
            }

            return Exclui(entidade) ? entidade : null;
        }

        public virtual bool Exclui(params T[] entidades)
        {
            return ProcessarExclusao(entidades, () => MakeCrudTransaction(repo =>
            {
                foreach (var entidade in entidades)
                {
                    RegistrarAuditoria(repo, entidade, null, "deletar");
                    repo.Exclui(entidade);
                }
            }));
        }

        public virtual async Task<T> ExcluiAsync(long id)
        {
            var entidade = await RetornaAsync(id);
            if (entidade == null)
            {
                Mensagens.Add("Registro não encontrado.");
                return null;
            }

            return await ExcluiAsync(entidade) ? entidade : null;
        }

        public virtual async Task<bool> ExcluiAsync(params T[] entidades)
        {
            return await ProcessarExclusaoAsync(entidades, async () => await MakeCrudTransactionAsync(async repo =>
            {
                foreach (var entidade in entidades)
                {
                    await RegistrarAuditoriaAsync(repo, entidade, null, "deletar");
                    await repo.ExcluiAsync(entidade);
                }
            }));
        }

        protected virtual bool ProcessarExclusao(T[] entidades, Func<bool> process)
        {
            if (entidades.Length == 0)
                return true;

            foreach (var entidade in entidades)
            {
                try { PreExclui?.Invoke(new ExcluiInfo<T>(entidade)); } catch (Exception ex) { Mensagens.Add(ex.Message); }
            }

            var sucesso = process();
            if (sucesso)
            {
                foreach (var entidade in entidades)
                {
                    try { PosExclui?.Invoke(new ExcluiInfo<T>(entidade)); } catch (Exception ex) { Mensagens.Add(ex.Message); }
                }
            }

            return sucesso;
        }

        protected virtual async Task<bool> ProcessarExclusaoAsync(T[] entidades, Func<Task<bool>> process)
        {
            if (entidades.Length == 0)
                return true;

            foreach (var entidade in entidades)
            {
                try { PreExclui?.Invoke(new ExcluiInfo<T>(entidade)); } catch (Exception ex) { Mensagens.Add(ex.Message); }
            }

            var sucesso = await process();
            if (sucesso)
            {
                foreach (var entidade in entidades)
                {
                    try { PosExclui?.Invoke(new ExcluiInfo<T>(entidade)); } catch (Exception ex) { Mensagens.Add(ex.Message); }
                }
            }

            return sucesso;
        }

        protected virtual void RegistrarAuditoria(IRepositorio repositorio, T entidadeAnterior, T entidadeAtual, string tipoOperacao)
        {
            var logEntidade = CriarLogEntidade(entidadeAnterior, entidadeAtual, tipoOperacao);
            if (logEntidade == null)
            {
                return;
            }

            repositorio.Inclui(logEntidade);

            foreach (var logPropriedade in CriarLogsPropriedade(logEntidade, entidadeAnterior, entidadeAtual))
            {
                repositorio.Inclui(logPropriedade);
            }
        }

        protected virtual async Task RegistrarAuditoriaAsync(IRepositorio repositorio, T entidadeAnterior, T entidadeAtual, string tipoOperacao)
        {
            var logEntidade = CriarLogEntidade(entidadeAnterior, entidadeAtual, tipoOperacao);
            if (logEntidade == null)
            {
                return;
            }

            await repositorio.IncluiAsync(logEntidade);

            foreach (var logPropriedade in CriarLogsPropriedade(logEntidade, entidadeAnterior, entidadeAtual))
            {
                await repositorio.IncluiAsync(logPropriedade);
            }
        }

        protected virtual LogEntidade CriarLogEntidade(T entidadeAnterior, T entidadeAtual, string tipoOperacao)
        {
            if ((entidadeAnterior is LogEntidade) || (entidadeAtual is LogEntidade) ||
                (entidadeAnterior is LogPropriedade) || (entidadeAtual is LogPropriedade))
            {
                return null;
            }

            var entidadeBase = entidadeAtual ?? entidadeAnterior;
            if (entidadeBase == null)
            {
                return null;
            }

            return new LogEntidade
            {
                NomeEntidade = typeof(T).Name,
                IdEntidade = entidadeBase.Id,
                TipoOperacao = tipoOperacao,
                DataOperacao = DateTime.Now,
                Usuario = CurrentUserContext?.UserName ?? string.Empty,
                IdUsuario = CurrentUserContext?.UserId ?? string.Empty
            };
        }

        protected virtual IEnumerable<LogPropriedade> CriarLogsPropriedade(LogEntidade logEntidade, T entidadeAnterior, T entidadeAtual)
        {
            foreach (var propriedadeMudada in ObterPropriedadesMudadas(entidadeAnterior, entidadeAtual))
            {
                yield return new LogPropriedade
                {
                    LogEntidade = logEntidade,
                    Propriedade = propriedadeMudada.Propriedade,
                    ValorAnterior = propriedadeMudada.ValorAnterior,
                    ValorAtual = propriedadeMudada.ValorAtual
                };
            }
        }

        protected virtual IEnumerable<PropriedadeMudadaInfo> ObterPropriedadesMudadas(T entidadeAnterior, T entidadeAtual)
        {
            var propriedades = typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(prop => prop.CanRead)
                .Where(prop => prop.GetIndexParameters().Length == 0)
                .Where(prop => EhTipoAuditavel(prop.PropertyType));

            foreach (var propriedade in propriedades)
            {
                var valorAnterior = entidadeAnterior != null ? propriedade.GetValue(entidadeAnterior) : null;
                var valorAtual = entidadeAtual != null ? propriedade.GetValue(entidadeAtual) : null;

                if (!ValoresDiferentes(valorAnterior, valorAtual))
                {
                    continue;
                }

                yield return new PropriedadeMudadaInfo
                {
                    Propriedade = propriedade.Name,
                    ValorAnterior = ConverterValorParaString(valorAnterior),
                    ValorAtual = ConverterValorParaString(valorAtual)
                };
            }
        }

        protected virtual bool ValoresDiferentes(object valorAnterior, object valorAtual)
        {
            return !Equals(valorAnterior, valorAtual);
        }

        protected virtual bool EhTipoAuditavel(Type tipo)
        {
            var tipoReal = Nullable.GetUnderlyingType(tipo) ?? tipo;

            if (tipoReal.IsEnum)
                return true;

            if (tipoReal.IsPrimitive)
                return true;

            return tipoReal == typeof(string)
                || tipoReal == typeof(decimal)
                || tipoReal == typeof(DateTime)
                || tipoReal == typeof(DateOnly)
                || tipoReal == typeof(TimeOnly)
                || tipoReal == typeof(TimeSpan)
                || tipoReal == typeof(Guid);
        }

        protected virtual string ConverterValorParaString(object valor)
        {
            if (valor == null)
                return null;

            return valor switch
            {
                DateTime dateTime => dateTime.ToString("O", CultureInfo.InvariantCulture),
                DateOnly dateOnly => dateOnly.ToString("O", CultureInfo.InvariantCulture),
                TimeOnly timeOnly => timeOnly.ToString("O", CultureInfo.InvariantCulture),
                TimeSpan timeSpan => timeSpan.ToString("c", CultureInfo.InvariantCulture),
                bool booleano => booleano.ToString(CultureInfo.InvariantCulture),
                IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
                _ => valor.ToString()
            };
        }

        protected class PropriedadeMudadaInfo
        {
            public string Propriedade { get; set; }
            public string ValorAnterior { get; set; }
            public string ValorAtual { get; set; }
        }
    }
}
