using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Infrastructure.Cache;
using Nop.Core.Caching;
using Nop.Services.Catalog;
using Nop.Services.Vendors;

namespace Nop.Web.Areas.Admin.Helpers
{
    /// <summary>
    /// Log creation helper
    /// </summary>
    public static class LogHelper
    {
        /// <summary>
        /// Create entity log
        /// </summary>
        /// <param name="email">Email of user changing value</param>
        /// <param name="id">Id of user changing value</param>
        /// <param name="entity">Entity being changed</param>
        /// <param name="entity">Old value of entity</param>
        /// <param name="entity">New value of entity</param>
        /// <returns>string</returns>
        public static string ChangedEntityLog(string email, string id, string entity, string oldValue, string newValue, bool isBadge = false)
        {
            var log = string.Empty;
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(id) ||
                string.IsNullOrEmpty(entity) || string.IsNullOrEmpty(oldValue) ||
                string.IsNullOrEmpty(newValue))
                return log;
            if (!isBadge)
                log = $"\n{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {email} ({id}) cambió {entity} de {oldValue} a {newValue}";
            else
                log = $"\n{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - Se cambió {entity} de {oldValue} a {newValue} para {email} ({id})";

            return log;
        }

        /// <summary>
        /// Create entity log when creating new entity
        /// </summary>
        /// <param name="email">Email of user changing value</param>
        /// <param name="id">Id of user changing value</param>
        /// <param name="name">Name/type of entity</param>
        /// <param name="entityName">Name assigned to entity</param>
        /// <param name="entityId">Id of entity</param>
        /// <returns>string</returns>
        public static string CreatingLog(string email, string id, string name, string entityName, string entityId, bool isBadge = false)
        {
            var log = string.Empty;
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(id) ||
                string.IsNullOrEmpty(name) || string.IsNullOrEmpty(entityName) ||
                string.IsNullOrEmpty(entityId))
                return log;
            if (!isBadge)
                log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {email} ({id}) creó {name} con nombre {entityName} ({entityId})";
            else
                log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - Se creó una/un {name} ({entityId}) para {email} ({id})";

            return log;
        }

        /// <summary>
        /// Create entity log when deliting entity
        /// </summary>
        /// <param name="email">Email of user changing value</param>
        /// <param name="id">Id of user changing value</param>
        /// <param name="name">Name/type of entity</param>
        /// <param name="entityName">Name assigned to entity</param>
        /// <param name="entityId">Id of entity</param>
        /// <returns>string</returns>
        public static string DeletingLog(string email, string id, string name, string entityName, string entityId, bool isBadge = false)
        {
            var log = string.Empty;
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(id) ||
                string.IsNullOrEmpty(name) || string.IsNullOrEmpty(entityName) ||
                string.IsNullOrEmpty(entityId))
                return log;
            if (!isBadge)
                log = $"\n{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {email} ({id}) eliminó {name} con nombre {entityName} ({entityId})";
            else
                log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - Se eliminó una/un {name} ({entityId}) para {email} ({id})";

            return log;
        }
    }
}