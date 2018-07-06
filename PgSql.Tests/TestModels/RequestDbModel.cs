using System;
using System.Collections.Generic;
using System.Text;
using doob.PgSql;
using doob.PgSql.Attributes;

namespace PgSql.Tests.TestModels
{
    public class RequestDbModel
    {

        public RequestDbModel()
        {

            var rule = new RequestPermissionRule();
            rule.AccessRights = RequestPermissionRights.List | RequestPermissionRights.Read;
            rule.SourceType = PermissionRuleSourceType.Role;
            rule.Value = "*";

            Permissions.Add(rule);
        }

        [PgSqlPrimaryKey(DefaultValues.Guid.New)]
        public Guid Id { get; set; }

        public string Type { get; set; }

        public string Requester { get; set; }

        public Dictionary<string, object> Parameters { get; set; }

        public RequestStatus RequestStatus { get; set; }

        public List<RequestPermissionRule> Permissions { get; set; } = new List<RequestPermissionRule>();

        [PgSqlDefaultValue(DefaultValues.DateTime.Now)]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
