using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace doob.PgSql.ExtensionMethods
{
    public static class EnumExtensions
    {

        public static string GetName(this Enum enumValue)
        {


            var enums = Enum.GetValues(enumValue.GetType()).Cast<Enum>().Where(enumValue.HasFlag);

            return String.Join(",", enums.Select(GetSingleName));
            //var type = enumValue.GetType();
            //var typeInfo = type.GetTypeInfo();
            //var declaredMembers = typeInfo.DeclaredMembers;

           


            //return enumValue.GetType()
            //                .GetTypeInfo()
            //                .DeclaredMembers
            //                .SingleOrDefault(x => x.Name == enumValue.ToString())
            //                .GetCustomAttribute<EnumMemberAttribute>(false)?.Value ?? enumValue.ToString();
        }

        private static string GetSingleName(Enum enumValue)
        {
            return enumValue.GetType()
                       .GetTypeInfo()
                       .DeclaredMembers
                       .SingleOrDefault(x => x.Name == enumValue.ToString())
                       .GetCustomAttribute<EnumMemberAttribute>(false)?.Value ?? enumValue.ToString();
        }

    }

}
