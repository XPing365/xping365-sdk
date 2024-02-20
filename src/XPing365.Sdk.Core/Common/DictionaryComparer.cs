namespace XPing365.Sdk.Core.Common
{
    internal static class PropertyBagComparer
    {
        // Define a custom method that takes two dictionaries as parameters
        public static bool CompareDictionaries(Dictionary<string, string> dict1, Dictionary<string, string> dict2)
        {
            ArgumentNullException.ThrowIfNull(dict1, nameof(dict1));
            ArgumentNullException.ThrowIfNull(dict2, nameof(dict2));

            // Check if the dictionaries have the same number of items
            if (dict1.Count != dict2.Count)
            {
                return false; // If not, they are not equal
            }
            // Loop over the key-value pairs of the first dictionary
            foreach (var pair in dict1)
            {
                // Get the key and value
                string key = pair.Key;
                string value = pair.Value;
                // Check if the second dictionary contains the same key
                if (!dict2.TryGetValue(key, out _))
                {
                    return false; // If not, they are not equal
                }
                // Check if the second dictionary has the same value for the key
                if (!value.Equals(value, StringComparison.Ordinal))
                {
                    return false; // If not, they are not equal
                }
            }
            // If the loop completes without returning false, the dictionaries are equal
            return true;
        }
    }
}
