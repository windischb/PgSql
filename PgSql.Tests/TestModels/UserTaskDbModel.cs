using System;
using System.Collections.Generic;
using System.Text;
using doob.PgSql;
using doob.PgSql.Attributes;

namespace PgSql.Tests.TestModels
{
    public class UserTaskDbModel
    {
        [PgSqlPrimaryKey(DefaultValues.Guid.New)]
        public Guid Id { get; set; }

        public string TaskInstanceId { get; set; }
        public string TaskDefinitionId { get; set; }
        public string TaskDefinitionName { get; set; }

        public string ProcessInstanceId { get; set; }
        public string ProcessDefinitionId { get; set; }
        public string ProcessDefinitionName { get; set; }
        public string ProcessDefinitionVersion { get; set; }

        public Dictionary<string, object> InputParameter { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object> OutputParameter { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object> MetaData { get; set; } = new Dictionary<string, object>();

        public string AssignedTo { get; set; }

        public string Responsible { get; set; }

        public List<UserTaskPermissionRule> Permissions { get; set; } = new List<UserTaskPermissionRule>();

        public UserTaskStatus UserTaskStatus { get; set; } = UserTaskStatus.New;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

    }
}
