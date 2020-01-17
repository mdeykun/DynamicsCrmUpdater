using Microsoft.VisualStudio.Settings;
using System;

namespace CrmWebResourcesUpdater.Common.Extensions
{
    /// <summary>
    /// Extensions for writable settings store
    /// </summary>
    public static class WritableSettingsStoreExtensions
    {
        /// <summary>
        /// Returns the value of the requested property whose data type is System.Uri
        /// </summary>
        /// <param name="store">Extending class</param>
        /// <param name="collectionPath">Path of the collection of the property</param>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>Uri or null if not set or not valid uri</returns>
        public static Uri GetUri(this WritableSettingsStore store, string collectionPath, string propertyName)
        {
            if (store.PropertyExists(collectionPath, propertyName))
            {
                Uri uri = null;
                string value = store.GetString(collectionPath, propertyName);
                Uri.TryCreate(value, UriKind.Absolute, out uri);
                return uri;
            }
            return null;
        }

        /// <summary>
        /// Returns the value of the requested property whose data type is System.Guid
        /// </summary>
        /// <param name="store">Extending class</param>
        /// <param name="collectionPath">Path of the collection of the property</param>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>Guid or null if not set or not valid uri</returns>
        public static Guid? GetGuid(this WritableSettingsStore store, string collectionPath, string propertyName)
        {
            if (store.PropertyExists(collectionPath, propertyName))
            {
                var guid = Guid.Empty;
                var value = store.GetString(collectionPath, propertyName);
                if(Guid.TryParse(value, out guid))
                {
                    return guid;
                }
            }
            return null;
        }

        /// <summary>
        /// Updates the value of the specified property to the given uri value while setting its data type to System.Uri
        /// </summary>
        /// <param name="store">Extending class</param>
        /// <param name="collectionPath">Path of the collection of the property</param>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="value">Value of the property</param>
        public static void SetUri(this WritableSettingsStore store, string collectionPath, string propertyName, Uri value)
        {
            if (value == null)
            {
                store.DeletePropertyIfExists(collectionPath, propertyName);

            }
            else
            {
                store.SetString(collectionPath, propertyName, value.ToString());
            }
        }

        /// <summary>
        /// Updates the value of the specified property to the given guid value while setting its data type to System.Guid
        /// </summary>
        /// <param name="store">Extending class</param>
        /// <param name="collectionPath">Path of the collection of the property</param>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="value">Value of the property</param>
        public static void SetGuid(this WritableSettingsStore store, string collectionPath, string propertyName, Guid? value)
        {
            if(value == null)
            {
                store.DeletePropertyIfExists(collectionPath, propertyName);
                
            }
            else
            {
                store.SetString(collectionPath, propertyName, value.ToString());
            }
        }

        /// <summary>
        /// Deletes property from settings store if it exists within specified collection
        /// </summary>
        /// <param name="store">Extending class</param>
        /// <param name="collectionPath">Path of the collection of the property</param>
        /// <param name="propertyName">Name of the property</param>
        public static void DeletePropertyIfExists(this WritableSettingsStore store, string collectionPath, string propertyName)
        {
            if (store.PropertyExists(collectionPath, propertyName))
            {
                store.DeleteProperty(collectionPath, propertyName);
            }
        }
    }
}
