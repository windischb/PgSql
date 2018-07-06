using doob.PgSql.Attributes;

namespace PgSql.Tests.TestModels.Camunda
{
    public class ProcessDefinitionDbModel
    {
        [PgSqlColumn("id_")]
        public string Id { get; set; }

        [PgSqlColumn("rev_")]
        public int Revision { get; set; }

        [PgSqlColumn("category_")]
        public string Category { get; set; }

        [PgSqlColumn("name_")]
        public string Name { get; set; }

        [PgSqlColumn("key_")]
        public string Key { get; set; }

        [PgSqlColumn("version_")]
        public int Version { get; set; }

        [PgSqlColumn("deployment_id_")]
        public string DeploymentId { get; set; }

        [PgSqlColumn("resource_name_")]
        public string ResourceName { get; set; }

        [PgSqlColumn("dgrm_resource_name_")]
        public string DgrmResourceName { get; set; }

        [PgSqlColumn("has_start_form_key_")]
        public bool HasStartFormKey { get; set; }

        [PgSqlColumn("suspension_state_")]
        public int? SuspensionState { get; set; }

        [PgSqlColumn("tenant_id_")]
        public string TenantId { get; set; }

        [PgSqlColumn("version_tag_")]
        public string VersionTag { get; set; }

        [PgSqlColumn("history_ttl_")]
        public int? HistoryTTL { get; set; }
    }
}
