using FGB.IRepositorios;
using Microsoft.Extensions.Logging;
using NHibernate;

namespace FGB.Dominio.Repositorios
{
	public class Migracao : IMigracao
	{
		private readonly ISessionFactory _sessionFactory;
		private readonly ILogger<Migracao> _logger;
		private const string TabelaMigracoes = "migracao_banco";
		private const string ColunaNomeArquivo = "nome_arquivo";

		public Migracao(ISessionFactory sessionFactory, ILogger<Migracao> logger)
		{
			_sessionFactory = sessionFactory;
			_logger = logger;
		}

		public void UpdateDatabase(string migrationsFolder)
		{
			var fullPath = ResolveMigrationsFolder(migrationsFolder);
			if (!Directory.Exists(fullPath))
			{
				throw new DirectoryNotFoundException("Pasta de migracoes nao encontrada: " + fullPath);
			}

			_logger.LogInformation("Executando migracoes a partir de: {Pasta}", fullPath);
			EnsureMigrationTable();

			var alreadyApplied = GetAppliedMigrations();
			var files = Directory
				.GetFiles(fullPath)
				.Where(path => path.EndsWith(".sql", StringComparison.OrdinalIgnoreCase)
					|| path.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
				.OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
				.ToList();

			foreach (var file in files)
			{
				var fileName = Path.GetFileName(file);
				if (alreadyApplied.Contains(fileName))
				{
					_logger.LogInformation("Pulando migracao ja aplicada: {Migracao}", fileName);
					continue;
				}

				if (ApplyMigrationFile(file, fileName))
				{
					alreadyApplied.Add(fileName);
				}
			}

			_logger.LogInformation("Migracoes finalizadas com sucesso.");
		}

		private void EnsureMigrationTable()
		{
			var ddlCriacao = @"CREATE TABLE IF NOT EXISTS migracao_banco (
                id BIGSERIAL PRIMARY KEY,
                data_execucao TIMESTAMP NOT NULL DEFAULT current_timestamp,
                nome_arquivo VARCHAR(255) NOT NULL UNIQUE
            );";

			using var session = _sessionFactory.OpenSession();
			using var transaction = session.BeginTransaction();
			try
			{
				session.CreateSQLQuery(ddlCriacao).ExecuteUpdate();
				transaction.Commit();
			}
			catch
			{
				transaction.Rollback();
				throw;
			}
		}

		private HashSet<string> GetAppliedMigrations()
		{
			using var session = _sessionFactory.OpenSession();
			var names = session
				.CreateSQLQuery($"SELECT {ColunaNomeArquivo} FROM {TabelaMigracoes} ORDER BY {ColunaNomeArquivo}")
				.List<string>();

			return new HashSet<string>(names ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
		}

		private bool ApplyMigrationFile(string path, string fileName)
		{
			var sql = File.ReadAllText(path).Trim();
			if (string.IsNullOrWhiteSpace(sql))
			{
				_logger.LogInformation("Pulando arquivo de migracao vazio: {Migracao}", fileName);
				return false;
			}

			using var session = _sessionFactory.OpenSession();
			using var transaction = session.BeginTransaction();
			try
			{
				// Claim migration execution atomically to prevent parallel instances from running the same script.
				var insert = session.CreateSQLQuery($"INSERT INTO {TabelaMigracoes} ({ColunaNomeArquivo}) VALUES (:nomeArquivo) ON CONFLICT ({ColunaNomeArquivo}) DO NOTHING");
				insert.SetParameter("nomeArquivo", fileName);
				var insertedRows = insert.ExecuteUpdate();

				if (insertedRows == 0)
				{
					transaction.Rollback();
					_logger.LogInformation("Pulando migracao aplicada em paralelo por outra instancia: {Migracao}", fileName);
					return false;
				}

				// Execute script exactly as provided; each file runs atomically in its own transaction.
				session.CreateSQLQuery(sql).ExecuteUpdate();

				transaction.Commit();
				_logger.LogInformation("Migracao aplicada com sucesso: {Migracao}", fileName);
				return true;
			}
			catch (Exception ex)
			{
				transaction.Rollback();
				_logger.LogError(ex, "Falha ao aplicar migracao: {Migracao}", fileName);
				throw;
			}
		}

		private static string ResolveMigrationsFolder(string migrationsFolder)
		{
			if (Path.IsPathRooted(migrationsFolder) && Directory.Exists(migrationsFolder))
			{
				return migrationsFolder;
			}

			var baseDir = AppContext.BaseDirectory;
			var directCandidate = Path.Combine(baseDir, migrationsFolder);
			if (Directory.Exists(directCandidate))
			{
				return directCandidate;
			}

			var current = new DirectoryInfo(baseDir);
			while (current != null)
			{
				var candidate1 = Path.Combine(current.FullName, "ConectaSTI.Dominio", migrationsFolder);
				if (Directory.Exists(candidate1))
				{
					return candidate1;
				}

				var candidate2 = Path.Combine(current.FullName, "ConectaSTI", "ConectaSTI.Dominio", migrationsFolder);
				if (Directory.Exists(candidate2))
				{
					return candidate2;
				}

				current = current.Parent;
			}

			return directCandidate;
		}
	}
}
