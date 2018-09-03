using System;
using doob.PgSql.Json.ContractResolvers;
using doob.PgSql.Json.Converters;
using Newtonsoft.Json.Converters;
using Reflectensions.JsonConverters;
using ExpandoObjectConverter = Newtonsoft.Json.Converters.ExpandoObjectConverter;

namespace doob.PgSql
{
    public static class Converter
    {

        private static readonly Lazy<Reflectensions.Json> lazyJson = new Lazy<Reflectensions.Json>(() => new Reflectensions.Json()
                                                                                                                .SetContractResolver<PrivateSetterContractResolver>()
                                                                                                                .UnRegisterJsonConverter<DefaultDictionaryConverter>()
                                                                                                                .RegisterJsonConverter<StringEnumConverter>()
                                                                                                                .RegisterJsonConverter<PgSqlLTreeConverter>()
                                                                                                                .RegisterJsonConverter<IpAddressConverter>()
                                                                                                                .RegisterJsonConverter<IpEndPointConverter>()
                                                                                                                .RegisterJsonConverter<DefaultDictionaryConverter>()
                                                                                                                .RegisterJsonConverter<ExpandoObjectConverter>()
        );

        public static Reflectensions.Json Json => lazyJson.Value;


    }

}
