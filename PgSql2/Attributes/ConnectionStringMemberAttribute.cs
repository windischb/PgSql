using System;

namespace PgSql2.Attributes
{
    internal class ConnectionStringMemberAttribute : Attribute
    {

        internal string Name { get; }
        
        public ConnectionStringMemberAttribute() { }
        public ConnectionStringMemberAttribute(string name)
        {
            Name = name;
        }
    }

    
}
