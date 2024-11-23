using System.Collections.Generic;

namespace jshepler.ngu.hardcore.ModSave
{
    internal static class Data
    {
        internal static Dictionary<string, object> Values = new();

        internal static T Get<T>(string key, T defaultValue = default)
        {
            if (!Values.ContainsKey(key))
                Values[key] = defaultValue;

            return (T)Values[key];
        }

        internal static void Set(string key, object value)
        {
            if (!Values.ContainsKey(key))
                Values.Add(key, value);
            else
                Values[key] = value;
        }

        internal static bool HardCore
        {
            get => Get("HardCore", false);
            set => Set("HardCore", value);
        }

        // REMINDER: NO TYPES DEFINED IN MODS ELSE NOT LOADABLE IN VANILLA
    }
}
