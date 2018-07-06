using System;
using System.Collections.Generic;
using System.Text;

namespace PgSql.Tests.TestModels
{
    public enum RequestStatus
    {
        New,
        Queued,
        InProgress,
        Suspended,
        Completed,
        Error,
        Retried,
        CompletedWithError
    }
}
