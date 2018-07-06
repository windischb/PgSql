using System;
using doob.PgSql.Attributes;

namespace XACamundaManager.ExternalTasks
{
    public class ExternalTaskDbModel {

        [PgSqlPrimaryKey]
        [PgSqlColumn("id_")]
        public string Id { get; set; }

        [PgSqlColumn("rev_")]
        public int Revision { get; set; }

        [PgSqlColumn("worker_id_")]
        public string WorkerId { get; set; }

        [PgSqlColumn("topic_name_")]
        public string TopicName { get; set; }

        [PgSqlColumn("retries_")]
        public int? Retries { get; set; }

        [PgSqlColumn("error_msg_")]
        public string ErrorMessage { get; set; }

        [PgSqlColumn("error_details_id_")]
        public string ErrorDetailsId { get; set; }

        [PgSqlColumn("lock_exp_time_")]
        public DateTime? LockExpirationTime { get; set; }

        [PgSqlColumn("suspension_state_")]
        public int? SuspensionState { get; set; }

        [PgSqlColumn("execution_id_")]
        public string ExecutionId { get; set; }

        [PgSqlColumn("proc_inst_id_")]
        public string ProcessInstanceId { get; set; }

        [PgSqlColumn("proc_def_id_")]
        public string ProcessDefinitionId { get; set; }

        [PgSqlColumn("proc_def_key_")]
        public string ProcessDefinitionKey { get; set; }

        [PgSqlColumn("act_id_")]
        public string ActivityId { get; set; }

        [PgSqlColumn("act_inst_id_")]
        public string ActivityInstanceId { get; set; }

        [PgSqlColumn("tenant_id_")]
        public string TenantId { get; set; }

        [PgSqlColumn("priority_")]
        public long Priority { get; set; }
    }
}
