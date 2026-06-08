using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Utilities
{
    public static class GetValueExtension
    {
        public static T GetJsonValue<T>(this IDictionary<string, object> data, string key)
        {
            if (data == null)
            {
                Debug.LogError($"Data is null.");
                return default;
            }

            if (!data.TryGetValue(key, out var obj)) return default;

            if (obj is JObject jObject)
            {
                return jObject.ToObject<T>();
            }

            if (obj is JArray jArray)
            {
                return jArray.ToObject<T>();
            }

            if (typeof(T).IsEnum)
            {
                return (T) Enum.Parse(typeof(T), obj.ToString());
            }
            
            return (T) Convert.ChangeType(obj, typeof(T));
        }

        public static T ToObject<T>(this object data)
        {
            if (data == null)
            {
                Debug.LogError($"Data is null.");
                return default;
            }

            if (data is JObject jObject)
            {
                return jObject.ToObject<T>();
            }

            if (data is JArray jArray)
            {
                return jArray.ToObject<T>();
            }

            if (typeof(T).IsEnum)
            {
                return (T) Enum.Parse(typeof(T), data.ToString());
            }

            return (T) Convert.ChangeType(data, typeof(T));
        }
    }
}
