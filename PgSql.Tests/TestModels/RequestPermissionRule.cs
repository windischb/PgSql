using System;
using System.Collections.Generic;
using System.Text;

namespace PgSql.Tests.TestModels
{
    public class RequestPermissionRule
    {
        public string Value { get; set; }
        public PermissionRuleSourceType SourceType { get; set; }
        public RequestPermissionRights AccessRights { get; set; }
    }

    [Flags]
    public enum RequestPermissionRights
    {
        List = 1,
        Read = 2
    }
}
