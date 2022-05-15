using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace PodNoms.Common.Persistence.Extensions {
    public static class DbContextExtensions {
        public static async Task<int> CountByRawSql(this DbContext dbContext, string sql,
            params KeyValuePair<string, object>[] parameters) {
            var reader = await GetReader(dbContext, sql, parameters);
            return Convert.ToInt32(reader ?? -1);
        }

        public static async Task<string> ExecuteScalar(this DbContext dbContext, string sql,
            params KeyValuePair<string, object>[] parameters) {
            var reader = await GetReader(dbContext, sql, parameters);
            return Convert.ToString(reader ?? string.Empty);
        }

        public static async Task<object> GetReader(this DbContext dbContext, string sql,
            params KeyValuePair<string, object>[] parameters) {
            await using var connection = dbContext.Database.GetDbConnection() as SqlConnection;
            try {
                connection.Open();

                await using var command = connection.CreateCommand();
                command.CommandText = sql;

                foreach (var parameter in parameters)
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);

                await using DbDataReader dataReader = await command.ExecuteReaderAsync();
                if (dataReader.Read())
                    return dataReader.GetValue(0);
            }
            // We should have better error handling here
            // Narrator: yes we should but what ya gonna do eh?
            catch (Exception) { } finally { connection.Close(); }

            return null;
        }

        public static IEnumerable<dynamic> CollectionFromSql(this DbContext dbContext, string Sql,
            Dictionary<string, object> Parameters) {
            using (var cmd = dbContext.Database.GetDbConnection().CreateCommand()) {
                cmd.CommandText = Sql;
                if (cmd.Connection.State != ConnectionState.Open)
                    cmd.Connection.Open();

                foreach (var param in Parameters) {
                    var dbParameter = cmd.CreateParameter();
                    dbParameter.ParameterName = param.Key;
                    dbParameter.Value = param.Value;
                    cmd.Parameters.Add(dbParameter);
                }

                //var retObject = new List<dynamic>();
                using (var dataReader = cmd.ExecuteReader()) {
                    while (dataReader.Read()) {
                        var dataRow = GetDataRow(dataReader);
                        yield return dataRow;
                    }
                }
            }
        }

        private static dynamic GetDataRow(DbDataReader dataReader) {
            var dataRow = new ExpandoObject() as IDictionary<string, object>;
            for (var fieldCount = 0; fieldCount < dataReader.FieldCount; fieldCount++)
                dataRow.Add(dataReader.GetName(fieldCount), dataReader[fieldCount]);
            return dataRow;
        }

        public static string ToSql<TEntity>(this IQueryable<TEntity> query) {
            var enumerator = query.Provider.Execute<IEnumerable<TEntity>>(query.Expression).GetEnumerator();
            var enumeratorType = enumerator.GetType();

            var selectFieldInfo = enumeratorType.GetField(
                                      "_selectExpression",
                                      BindingFlags.NonPublic | BindingFlags.Instance) ??
                                  throw new InvalidOperationException(
                                      $"cannot find field _selectExpression on type {enumeratorType.Name}");

            var sqlGeneratorFieldInfo = enumeratorType.GetField(
                                            "_querySqlGeneratorFactory",
                                            BindingFlags.NonPublic | BindingFlags.Instance) ??
                                        throw new InvalidOperationException(
                                            $"cannot find field _querySqlGeneratorFactory on type {enumeratorType.Name}");

            var selectExpression = selectFieldInfo.GetValue(enumerator) as SelectExpression ??
                                   throw new InvalidOperationException($"could not get SelectExpression");
            var factory = sqlGeneratorFieldInfo.GetValue(enumerator) as IQuerySqlGeneratorFactory ??
                          throw new InvalidOperationException($"could not get IQuerySqlGeneratorFactory");

            var sqlGenerator = factory.Create();
            var command = sqlGenerator.GetCommand(selectExpression);
            var sql = command.CommandText;

            return sql;
        }
    }
}
