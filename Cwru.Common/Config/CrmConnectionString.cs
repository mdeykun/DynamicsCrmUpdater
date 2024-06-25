using Cwru.Common.Attributes;
using Cwru.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security;

namespace Cwru.Common.Model
{
    public class CrmConnectionString
    {
        public CrmConnectionString()
        {
            OtherValues = new Dictionary<string, string>();
        }

        public Dictionary<string, string> OtherValues { get; set; }

        [ConnectionStringAliases("ServiceUri", "Service Uri", "Url", "Server")]
        public string ServiceUri { get; set; }
        public string ServiceUriAlias { get; set; }

        [ConnectionStringAliases("Domain")]
        public string Domain { get; set; }
        public string DomainAlias { get; set; }

        [ConnectionStringAliases("UserName", "User Name", "UserId", "User Id")]
        public string UserName { get; set; }
        public string UserNameAlias { get; set; }

        [ConnectionStringAliases("Password")]
        public SecureString Password { get; set; }
        public string PasswordAlias { get; set; }

        [ConnectionStringAliases("HomeRealmUri", "Home Realm Uri")]
        public string HomeRealmUri { get; set; }
        public string HomeRealmUriAlias { get; set; }

        [ConnectionStringAliases("AuthenticationType", "AuthType")]
        public AuthenticationType? AuthenticationType { get; set; }
        public string AuthenticationTypeAlias { get; set; }

        [ConnectionStringAliases("RequireNewInstance")]
        public bool? RequireNewInstance { get; set; } = null;
        public string RequireNewInstanceAlias { get; set; }

        [ConnectionStringAliases("ClientId", "AppId", "ApplicationId")]
        public Guid? ClientId { get; set; }
        public string ClientIdAlias { get; set; }

        [ConnectionStringAliases("RedirectUri", "ReplyUrl")]
        public string RedirectUri { get; set; }
        public string RedirectUriAlias { get; set; }

        [ConnectionStringAliases("TokenCacheStorePath")]
        public string TokenCacheStorePath { get; set; }
        public string TokenCacheStorePathAlias { get; set; }

        [ConnectionStringAliases("LoginPrompt")]
        public string LoginPrompt { get; set; }
        public string LoginPromptAlias { get; set; }

        [ConnectionStringAliases("SkipDiscovery")]
        public string SkipDiscovery { get; set; }
        public string SkipDiscoveryAlias { get; set; }

        [ConnectionStringAliases("Thumbprint", "CertificateThumbprint")]
        public string Thumbprint { get; set; }
        public string ThumbprintAlias { get; set; }

        [ConnectionStringAliases("StoreName", "CertificateStoreName")]
        public string StoreName { get; set; }
        public string StoreNameAlias { get; set; }

        [ConnectionStringAliases("ClientSecret", "Secret")]
        public SecureString ClientSecret { get; set; }
        public string ClientSecretAlias { get; set; }

        [ConnectionStringAliases("Integrated Security")]
        public bool? IntegratedSecurity { get; set; }
        public string IntegratedSecurityAlias { get; set; } = null;

        public static CrmConnectionString Parse(string connectionString)
        {
            var keyValues = connectionString
                .Split(';')
                .Where(kvp => kvp.Contains('='))
                .Select(kvp => kvp.Split(new char[] { '=' }, 2))
                .ToDictionary(kvp => kvp[0].Trim(), kvp => kvp[1].Trim(), StringComparer.OrdinalIgnoreCase);

            var connectionInfoProperties = typeof(CrmConnectionString).GetProperties();
            var propertiesWithAttribute = connectionInfoProperties.Where(prop => prop.IsDefined(typeof(ConnectionStringAliases), false));

            var cs = new CrmConnectionString();

            foreach (var property in propertiesWithAttribute)
            {
                var aliasesAttributes = (ConnectionStringAliases[])property.GetCustomAttributes(typeof(ConnectionStringAliases), false);
                var aliases = ConcatArrays(aliasesAttributes.Select(x => x.Aliases).ToArray());

                var key = keyValues.Keys.FirstOrDefault(x => aliases.Contains(x, StringComparer.OrdinalIgnoreCase));

                var aliasPropertyName = $"{property.Name}Alias";
                var aliasProperty = connectionInfoProperties.Where(prop => prop.Name == aliasPropertyName).FirstOrDefault();
                if (aliasProperty == null)
                {
                    throw new NotImplementedException($"Property with {aliasPropertyName} was not found on ConnectionInfo");
                }

                aliasProperty.SetValue(cs, key);

                if (key != null)
                {
                    var value = keyValues[key];
                    var type = property.PropertyType;
                    if (type == typeof(bool) || type == typeof(bool?))
                    {
                        if (string.Compare(value, "true", true) == 0)
                        {
                            property.SetValue(cs, true);
                        }

                        if (string.Compare(value, "false", true) == 0)
                        {
                            property.SetValue(cs, false);
                        }
                    }
                    else if (type == typeof(Guid) || type == typeof(Guid?))
                    {
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            var guid = Guid.Parse(value);
                            property.SetValue(cs, guid);
                        }
                    }
                    else if (type == typeof(SecureString))
                    {
                        property.SetValue(cs, value.ToSecureString(), null);
                    }
                    else if (type == typeof(string))
                    {
                        property.SetValue(cs, value);
                    }
                    else if (type == typeof(AuthenticationType?))
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            property.SetValue(cs, Enum.Parse(typeof(AuthenticationType), value));
                        }
                        else
                        {
                            property.SetValue(cs, null);
                        }
                    }
                    else if (type == typeof(AuthenticationType))
                    {
                        property.SetValue(cs, Enum.Parse(typeof(AuthenticationType), value));
                    }
                    else
                    {
                        throw new NotSupportedException($"Property of type {type.Name} is not supported");
                    }

                    keyValues.Remove(key);
                }
            }

            foreach (var keyValue in keyValues)
            {
                cs.OtherValues.Add(keyValue.Key, keyValue.Value);
            }

            return cs;
        }

        public string BuildConnectionString()
        {
            var csBuilder = new DbConnectionStringBuilder(true);

            var connectionInfoProperties = typeof(CrmConnectionString).GetProperties();
            var propertiesWithAttribute = connectionInfoProperties.Where(prop => prop.IsDefined(typeof(ConnectionStringAliases), false));
            foreach (var property in propertiesWithAttribute)
            {
                var propertyValue = property.GetValue(this);
                var stringValue = PropertyValueToString(propertyValue);
                if (stringValue != null)
                {
                    var aliasPropertyName = $"{property.Name}Alias";
                    var aliasProperty = connectionInfoProperties.Where(prop => prop.Name == aliasPropertyName).FirstOrDefault();
                    if (aliasProperty == null)
                    {
                        throw new NotImplementedException($"Property with {aliasPropertyName} was not found on ConnectionInfo");
                    }

                    var aliasPropertyValue = (string)aliasProperty.GetValue(this);
                    if (aliasPropertyValue != null && !string.IsNullOrEmpty(aliasPropertyValue))
                    {
                        csBuilder[aliasPropertyValue] = stringValue;
                    }
                    else
                    {
                        var aliasesAttributes = (ConnectionStringAliases[])property.GetCustomAttributes(typeof(ConnectionStringAliases), false);
                        var aliases = ConcatArrays(aliasesAttributes.Select(x => x.Aliases).ToArray());
                        if (aliases.Length == 0)
                        {
                            throw new Exception($"ConnectionStringAliases attribute is not set for property {property.Name}");
                        }
                        csBuilder[aliases[0]] = stringValue;
                    }
                }
            }

            foreach (var keyValue in OtherValues)
            {
                csBuilder.Add(keyValue.Key, keyValue.Value);
            }

            return csBuilder.ToString();
        }

        public static T[] ConcatArrays<T>(params T[][] list)
        {
            var result = new T[list.Sum(a => a.Length)];
            int offset = 0;
            for (int x = 0; x < list.Length; x++)
            {
                list[x].CopyTo(result, offset);
                offset += list[x].Length;
            }
            return result;
        }

        public override string ToString()
        {
            return BuildConnectionString();
        }

        public string PropertyValueToString(object value)
        {
            string result = null;

            if (value is Guid)
            {
                result = ((Guid)value).ToString("B");
            }

            if (value is bool?)
            {
                if (value != null)
                {
                    result = (bool?)value == true ? "True" : "False";
                }
            }

            if (value is Guid?)
            {
                result = ((Guid?)value)?.ToString("B");
            }

            if (value is string)
            {
                result = (string)value;
            }

            if (value is SecureString)
            {
                result = ((SecureString)value).GetString();
            }

            if (value is AuthenticationType?)
            {
                if (value != null)
                {
                    result = ((AuthenticationType?)value).Value.ToString();
                }
            }

            if (value is AuthenticationType)
            {
                result = value.ToString();
            }

            if (string.IsNullOrEmpty(result))
            {
                return null;
            }

            return result;
        }
    }
}
