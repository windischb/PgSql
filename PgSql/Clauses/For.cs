using System;
using System.Collections.Generic;
using System.Linq;
using doob.PgSql.Interfaces;

namespace doob.PgSql.Clauses
{
    public class For : ISelectMember
    {
        private ForType _forType;
        private ForOptions _forOptions;
        private List<string> _ofTables = new List<string>();

        private For()
        {
        }

        private For(ForType type)
        {
            _forType = type;
        }

        public static For Update()
        {
            return new For(ForType.Update);
        }

        public static For NoKeyUpdate()
        {
            return new For(ForType.NoKeyUpdate);
        }

        public static For Share()
        {
            return new For(ForType.Share);
        }

        public static For KeyShare()
        {
            return new For(ForType.KeyShare);
        }

        public For NoWait()
        {
            return SetOptions(ForOptions.NoWait);
        }

        public For SkipLocked()
        {
            return SetOptions(ForOptions.SkipLocked);
        }

        private For SetOptions(ForOptions options)
        {
            _forOptions = options;
            return this;
        }

        public For OfTables(params string[] tableName)
        {
            _ofTables = tableName.ToList();
            return this;
        }


        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var sqlCommand = new PgSqlCommand();


            switch (_forType)
            {
                case ForType.Update:
                    sqlCommand.AppendCommand("UPDATE");
                    break;
                case ForType.NoKeyUpdate:
                    sqlCommand.AppendCommand("NO KEY UPDATE");
                    break;
                case ForType.Share:
                    sqlCommand.AppendCommand("SHARE");
                    break;
                case ForType.KeyShare:
                    sqlCommand.AppendCommand("KEY SHARE");
                    break;
            }

            if (_ofTables.Any())
            {
                sqlCommand.AppendCommand($" OF {String.Join(",", _ofTables)}");
            }

            switch (_forOptions)
            {
                case ForOptions.None:
                    break;
                case ForOptions.NoWait:
                    sqlCommand.AppendCommand(" NOWAIT");
                    break;
                case ForOptions.SkipLocked:
                    sqlCommand.AppendCommand(" SKIP LOCKED");
                    break;
            }


            return sqlCommand;
        }
    }

    public enum ForType
    {
        Update,
        NoKeyUpdate,
        Share,
        KeyShare
    }

    public enum ForOptions
    {
        None,
        NoWait,
        SkipLocked
    }
}