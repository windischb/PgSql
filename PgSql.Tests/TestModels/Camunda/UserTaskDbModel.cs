using System;
using doob.PgSql.Attributes;

namespace PgSql.Tests.TestModels.Camunda
{
    public class UserTaskDbModel
    {
        [PgSqlColumn("id_")]
        public string Id { get; set; }

        [PgSqlColumn("name_")]
        public string Name { get; set; }

        [PgSqlColumn("rev_")]
        public int Revision { get; set; }

        [PgSqlColumn("execution_id_")]
        public string ExecutionId { get; set; }

        [PgSqlColumn("proc_inst_id_")]
        public string ProcessInstanceId { get; set; }

        [PgSqlColumn("proc_def_id_")]
        public string ProcessDefinitionId { get; set; }

        [PgSqlColumn("case_execution_id_")]
        public string CaseExecutionId { get; set; }

        [PgSqlColumn("case_inst_id_")]
        public string CaseInstanceId { get; set; }

        [PgSqlColumn("case_def_id_")]
        public string CaseDefinitionId { get; set; }

        [PgSqlColumn("parent_task_id_")]
        public string ParentTaskId { get; set; }

        [PgSqlColumn("description_")]
        public string Description { get; set; }

        [PgSqlColumn("task_def_key_")]
        public string TaskDefinitionKey { get; set; }

        [PgSqlColumn("owner_")]
        public string Owner { get; set; }

        [PgSqlColumn("assignee_")]
        public string Assignee { get; set; }

        [PgSqlColumn("delegation_")]
        public string Delegation { get; set; }

        [PgSqlColumn("priority_")]
        public int? Priority { get; set; }

        [PgSqlColumn("create_time_")]
        public DateTime? CreateTime { get; set; }

        [PgSqlColumn("due_date_")]
        public DateTime? DueDate { get; set; }

        [PgSqlColumn("follow_up_date_")]
        public DateTime? FollowUpDate { get; set; }

        [PgSqlColumn("suspension_state_")]
        public int? SuspensionState { get; set; }

        [PgSqlColumn("tenant_id_")]
        public string TenantId { get; set; }
    }
}
