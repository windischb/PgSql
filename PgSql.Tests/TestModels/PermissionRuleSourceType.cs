using System;
using System.Collections.Generic;
using System.Text;

namespace PgSql.Tests.TestModels
{
    public class UserTaskPermissionRule
    {
        public string Value { get; set; }
        public PermissionRuleSourceType SourceType { get; set; }
        public UserTaskPermissionRights AccessRights { get; set; }
    }

    [Flags]
    public enum UserTaskPermissionRights
    {
        List = 1,
        Read = 2,
        Handle = 4,
        Manage = 8,
        Terminate = 16
    }

    public enum PermissionRuleSourceType
    {
        User,
        Role
    }
}
