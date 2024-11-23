using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace jshepler.ngu.hardcore.ModSave
{
    internal class PlayerDataBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (typeName == "PlayerData")
                return typeof(mods.ModSave.ModPlayerData);

            return Assembly.Load(assemblyName).GetType(typeName);
        }
    }
}
