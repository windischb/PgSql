using System;
using System.Collections.Generic;
using System.Text;

namespace doob.PgSql.Listener
{
    public enum ConnectionCommand
    {
        Connect,
        Reconnect,
        Disconnect
    }
}
