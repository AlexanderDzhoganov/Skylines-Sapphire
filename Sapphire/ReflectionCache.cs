using System.Collections.Generic;
using System;
using System.Reflection;

namespace Sapphire
{
    public static class ReflectionCache
    {

        private static Dictionary<Type, Dictionary<string, PropertyInfo>> cache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();

        public static PropertyInfo GetPropertyForType(Type type, string name)
        {
            if (!cache.ContainsKey(type))
            {
                cache[type] = new Dictionary<string, PropertyInfo>(); 
            }

            if (!cache[type].ContainsKey(name))
            {
                cache[type][name] = type.GetProperty(name);
            }

            return cache[type][name];
        }

    }

}
