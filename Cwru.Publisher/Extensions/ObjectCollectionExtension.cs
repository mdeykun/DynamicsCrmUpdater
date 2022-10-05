using System.Linq;
using static System.Windows.Forms.ListBox;

namespace Cwru.Publisher.Extensions
{
    public static class ObjectCollectionExtension
    {
        public static T[] ToArray<T>(this ObjectCollection collection)
        {
            var selectedItems = new object[collection.Count];
            collection.CopyTo(selectedItems, 0);

            return selectedItems.Cast<T>().ToArray();
        }

        public static void RemoveRange<T>(this ObjectCollection collection, T[] itemsToRemove)
        {
            foreach (var item in itemsToRemove)
            {
                collection.Remove(item);
            }
        }

        public static T[] ToArray<T>(this SelectedObjectCollection collection)
        {
            var selectedItems = new object[collection.Count];
            collection.CopyTo(selectedItems, 0);

            return selectedItems.Cast<T>().ToArray();
        }
    }
}
