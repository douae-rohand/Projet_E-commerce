using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Projet__E_commerce.Data;
using System.Text.RegularExpressions;

namespace Projet__E_commerce.Services
{
    public class DatabaseMetadataService
    {
        private readonly ApplicationDbContext _db;
        private readonly string? _connectionString;

        public DatabaseMetadataService(ApplicationDbContext db, IConfiguration configuration)
        {
            _db = db;
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public async Task<object> GetSchemaAsync()
        {
            var schema = new List<object>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Get Tables and Columns
                var query = @"
                    SELECT 
                        t.TABLE_NAME, 
                        c.COLUMN_NAME, 
                        c.DATA_TYPE, 
                        c.IS_NULLABLE
                    FROM INFORMATION_SCHEMA.TABLES t
                    JOIN INFORMATION_SCHEMA.COLUMNS c ON t.TABLE_NAME = c.TABLE_NAME
                    WHERE t.TABLE_TYPE = 'BASE TABLE' 
                      AND t.TABLE_SCHEMA = 'dbo'
                    ORDER BY t.TABLE_NAME, c.ORDINAL_POSITION";

                using (var command = new SqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    string? currentTable = null;
                    List<object>? columns = null;

                    while (await reader.ReadAsync())
                    {
                        var tableName = reader.GetString(0);
                        if (tableName == "__EFMigrationsHistory") continue;

                        if (tableName != currentTable)
                        {
                            if (currentTable != null)
                            {
                                schema.Add(new { Table = currentTable, Columns = columns });
                            }
                            currentTable = tableName;
                            columns = new List<object>();
                        }

                        columns?.Add(new
                        {
                            Name = reader.GetString(1),
                            Type = reader.GetString(2),
                            Nullable = reader.GetString(3) == "YES"
                        });
                    }

                    if (currentTable != null)
                    {
                        schema.Add(new { Table = currentTable, Columns = columns ?? new List<object>() });
                    }
                }
            }

            return schema;
        }

        public async Task<object> ExecuteReadOnlyQueryAsync(string sql)
        {
            // Basic safety checks
            if (string.IsNullOrWhiteSpace(sql))
                return new { error = "Query is empty" };

            // Check for forbidden keywords (very simple check, can be bypassed but it's a first layer)
            var forbiddenPattern = @"\b(DROP|DELETE|UPDATE|INSERT|TRUNCATE|ALTER|CREATE|EXEC|EXECUTE)\b";
            if (Regex.IsMatch(sql, forbiddenPattern, RegexOptions.IgnoreCase))
            {
                return new { error = "Only SELECT queries are allowed for security reasons." };
            }

            if (!sql.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                return new { error = "Only SELECT queries are allowed." };
            }

            try
            {
                var results = new List<Dictionary<string, object>>();

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(sql, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        // Limit to top 100 results for safety
                        int count = 0;
                        while (await reader.ReadAsync() && count < 100)
                        {
                            var row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row[reader.GetName(i)] = reader.GetValue(i) ?? DBNull.Value;
                            }
                            results.Add(row);
                            count++;
                        }
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                return new { error = ex.Message };
            }
        }

        public object GetAvailableServices()
        {
            return new
            {
                Services = new[]
                {
                    new { Name = "Catalogue Produits", Description = "Recherche et détails des produits disponibles." },
                    new { Name = "Gestion des Commandes", Description = "Suivi et historique des commandes clients." },
                    new { Name = "Gestion Utilisateurs", Description = "Informations sur les clients et administrateurs." },
                    new { Name = "Analytique Ventes", Description = "Statistiques sur les ventes et performances par catégorie." },
                    new { Name = "Support Client", Description = "Consultation des avis et retours clients." }
                }
            };
        }
    }
}
