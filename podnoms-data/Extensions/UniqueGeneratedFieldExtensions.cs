﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Data.Annotations;
using PodNoms.Data.Interfaces;

namespace PodNoms.Data.Extensions {
    /// <summary>Class <c>UniqueGeneratedFieldExtensions</c>
    /// Various methods for slugifying/unique keying entities.</summary>
    ///
    public class GenerateSlugFailureException : Exception {
        public GenerateSlugFailureException(string message) : base(message) { }
    }

    public static class UniqueGeneratedFieldExtensions {
        /// <summary>
        /// Simple ViewModel to easily work with ExecSQL 
        /// </summary>
        private class ProxySluggedModel : ISluggedEntity {
            public string Slug { get; set; }
        }

        public static IEnumerable<T> Select<T>(this IDataReader reader,
            Func<IDataReader, T> projection) {
            while (reader.Read()) {
                yield return projection(reader);
            }
        }

        public static IEnumerable<T> ExecSQL<T>(this DbContext context, string query)
            where T : class, ISluggedEntity, new() {
            using (var command = context.Database.GetDbConnection().CreateCommand()) {
                command.CommandText = query;
                command.CommandType = CommandType.Text;
                context.Database.OpenConnection();

                using (var reader = command.ExecuteReader()) {
                    var result = reader.Select(r => new T {
                        Slug = r["Slug"] is DBNull ? string.Empty : r["Slug"].ToString()
                    });
                    return result.ToList();
                }
            }
        }

        public static string GenerateSlug(this IUniqueFieldEntity entity, DbContext context, ILogger logger = null) {
            try {
                var property = entity.GetType()
                    .GetProperties()
                    .FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(SlugFieldAttribute)));
                if (property != null) {
                    var attribute = property
                        .GetCustomAttributes(typeof(SlugFieldAttribute), false)
                        .FirstOrDefault();

                    var t = entity.GetType();
                    var tableName = context.Model.FindEntityType(t).GetTableName();
                    if (!string.IsNullOrEmpty(tableName)) {
                        var sourceField = (attribute as SlugFieldAttribute)?.SourceField;
                        if (string.IsNullOrEmpty(sourceField)) {
                            logger?.LogError($"Error slugifying - Entry title is blank, cannot slugify");
                            // need to throw here, shouldn't save without slug
                            throw new GenerateSlugFailureException("Entry title is blank, cannot slugify");
                        }

                        var slugSource = entity.GetType()
                            .GetProperty(sourceField)
                            ?.GetValue(entity, null)
                            ?.ToString() ?? string.Empty;

                        var source = context.ExecSQL<ProxySluggedModel>($"SELECT Slug FROM {tableName}")
                            .Select(m => m.Slug);

                        return slugSource.Slugify(source);
                    }
                }
            } catch (Exception ex) {
                logger?.LogError($"Error slugifying {entity.GetType().Name} - {ex.Message}");
                // need to throw here, shouldn't save without slug
                throw new GenerateSlugFailureException(ex.Message);
            }

            return string.Empty;
        }
    }
}
