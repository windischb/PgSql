using System;
using System.Collections.Generic;
using System.Text;

namespace PgSql.Tests.TestModels
{
    public enum UserTaskStatus
    {
        New, // neuer UserTask, wurde noch nicht assigned
        InProgress, // sobald ein neuer UserTask das erste mal assigned wird
        Completed, // abgeschlossener UserTask
        Canceled, // abgebrochener UserTask durch User
        Departed // "verstorbener" UserTask, zb. durch canceln des Tasks in Camunda
    }
}
