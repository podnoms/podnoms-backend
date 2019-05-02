using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PodNoms.Common.Persistence.Extensions {
    public static class DbContextExtensions {
        public static async Task<int> CountByRawSql(this DbContext dbContext, string sql, params KeyValuePair<string, object>[] parameters) {
            var reader = await GetReader(dbContext, sql, parameters);
            return Convert.ToInt32(reader ?? -1);
        }
        public static async Task<string> ExecuteScalar(this DbContext dbContext, string sql, params KeyValuePair<string, object>[] parameters) {
            var reader = await GetReader(dbContext, sql, parameters);
            return Convert.ToString(reader ?? string.Empty);
        }
        public static async Task<object> GetReader(this DbContext dbContext, string sql, params KeyValuePair<string, object>[] parameters) {
            using (var connection = dbContext.Database.GetDbConnection() as SqlConnection) {
                try {
                    connection.Open();

                    using (var command = connection.CreateCommand()) {
                        command.CommandText = sql;

                        foreach (var parameter in parameters)
                            command.Parameters.AddWithValue(parameter.Key, parameter.Value);

                        using (DbDataReader dataReader = await command.ExecuteReaderAsync()) {
                            if (dataReader.Read())
                                return dataReader.GetValue(0);
                        }
                    }
                }
                // We should have better error handling here
                // Narrator: yes we should but what ya gonna do eh?
                catch (Exception) { } finally { connection.Close(); }
            }
            return null;
        }
        public static IEnumerable<dynamic> CollectionFromSql(this DbContext dbContext, string Sql, Dictionary<string, object> Parameters) {
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
    }
}