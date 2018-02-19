using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using doob.PgSql.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace doob.PgSql.Json.ContractResolvers
{
    public class PrivateSetterContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var jProperty = base.CreateProperty(member, memberSerialization);

            
            if (member.IsDefined(typeof(PgSqlIncludeAttribute))) {
                jProperty.Ignored = false;
                jProperty.Readable = true;
            }

            if (member.IsDefined(typeof(PgSqlIgnoreAttribute))) {
                jProperty.Ignored = true;
                jProperty.Readable = false;
            }

            if (member.IsDefined(typeof(PgSqlColumnAttribute))) {
                jProperty.PropertyName = member.GetCustomAttribute<PgSqlColumnAttribute>().Name ?? member.Name;
            }
                

            if (jProperty.Writable)
                return jProperty;

            jProperty.Writable = member.IsPropertyWithSetter();

            return jProperty;
        }

        

        protected override List<MemberInfo> GetSerializableMembers(Type objectType) {

            var allMembers = objectType
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                               BindingFlags.Static)
                .Select(p => (MemberInfo) p);

            var defaultMembers = objectType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => (MemberInfo)p);

            List<MemberInfo> serializableMembers = new List<MemberInfo>();

            foreach (MemberInfo member in allMembers) {

                // exclude members that are compiler generated if set
                if (SerializeCompilerGeneratedMembers || !member.IsDefined(typeof(CompilerGeneratedAttribute), true))
                {
                    if (defaultMembers.Contains(member))
                    {
                        // add all members that are found by default member search
                        serializableMembers.Add(member);
                    }
                    else
                    {
                        // add members that are explicitly marked with JsonProperty/DataMember attribute
                        // or are a field if serializing just fields
                        if (member.IsDefined(typeof(PgSqlIgnoreAttribute), true)) {
                            continue;
                        }

                        if (member.IsDefined(typeof(PgSqlIncludeAttribute), true))
                        {
                            serializableMembers.Add(member);
                        }
                    }
                }
            }
            return serializableMembers;
        }

        
       
    }

    internal static class MemberInfoExtensions
    {
        internal static bool IsPropertyWithSetter(this MemberInfo member)
        {
            var property = member as PropertyInfo;

            return property?.GetSetMethod(true) != null;
        }
    }
}
