using Microsoft.VisualStudio.Settings;
using System;

namespace Cwru.Common.Extensions
{
    public static class WritableSettingsStoreExtensions
    {
        public static Guid? GetGuid(this WritableSettingsStore store, string collectionPath, string propertyName)
        {
            if (store.PropertyExists(collectionPath, propertyName))
            {
                var guid = Guid.Empty;
                var value = store.GetString(collectionPath, propertyName);
                if (Guid.TryParse(value, out guid))
                {
                    return guid;
                }
            }
            return null;
        }

        public static bool GetBoolOrDefault(this WritableSettingsStore store, string collectionPath, string propertyName)
        {
            if (store.PropertyExists(collectionPath, propertyName))
            {
                return store.GetBoolean(collectionPath, propertyName);
            }

            return false;
        }

        public static string GetStringOrDefault(this WritableSettingsStore store, string collectionPath, string propertyName)
        {
            if (store.PropertyExists(collectionPath, propertyName))
            {
                var value = store.GetString(collectionPath, propertyName);
                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }

            return null;
        }

    }
}
