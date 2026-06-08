using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

 namespace Enixan.GoldenDust.Extensions
 {
     public static class ArrayExtensions
     {
         public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, int size)
         {
             var i = 0;
             return
                 from element in source
                 group element by i++ / size into splitGroups
                 select splitGroups.AsEnumerable();
         }

         public static IEnumerable<T> Reversed<T>(this IEnumerable<T> list)
         {
             return list.Reverse();
         }

         public static IEnumerable<T> ToEnumerable<T>(this IEnumerator enumerator)
         {
             while (enumerator.MoveNext())
             {
                 yield return (T)enumerator.Current;
             }
         }

         public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator)
         {
             while (enumerator.MoveNext())
             {
                 yield return enumerator.Current;
             }
         }


         public static void ForEach<T>(this IEnumerable<T> first, Action<T> action)
         {
             foreach (T t in first)
             {
                 action(t);
             }
         }

         public static void ForEach<T>(this IEnumerable<T> first, Action<T, int> action)
         {
             var index = 0;
             foreach (T t in first)
             {
                 action(t, index++);
             }
         }


         public static T TryGet<T>(this List<T> list, int i)
         {
             if (i < 0 || list.Count <= i)
                 return default;
             return list[i];
         }

         public static List<T> Shuffle<T>(this List<T> list)
         {
             var n = list.Count;
             var newList = list.CopyList();
             while (n > 0)
             {
                 var k = Random.Range(0, n);
                 var item = newList[n - 1];
                 newList[n - 1] = newList[k];
                 newList[k] = item;
                 n--;
             }
             return newList;
         }

         public static void ShuffleInPlace<T>(this List<T> list)
         {
             var n = list.Count;
             while (n > 0)
             {
                 var k = Random.Range(0, n);
                 var item = list[n - 1];
                 list[n - 1] = list[k];
                 list[k] = item;
                 n--;
             }
         }

         public static T[] Shuffle<T>(this T[] array)
         {
             var n = array.Length;
             var newArray = array.CopyArray();
             while (n > 0)
             {
                 var k = Random.Range(0, n);
                 var item = newArray[n - 1];
                 newArray[n - 1] = newArray[k];
                 newArray[k] = item;
                 n--;
             }
             return newArray;
         }

         public static List<T> CopyList<T>(this List<T> list)
         {
             if (list == null)
                 return new List<T>();
             var n = list.Count;
             var newList = new List<T>();
             for (var i = 0; i < n; i++)
             {
                 newList.Add(list[i]);
             }
             return newList;
         }

         public static T[] CopyArray<T>(this T[] array)
         {
             var n = array.Length;
             var newArray = new T[n];
             for (var i = 0; i < n; i++)
             {
                 newArray[i] = array[i];
             }
             return newArray;
         }

         public static T GetRandomElement<T>(this IReadOnlyList<T> list)
         {
             if (list == null || list.Count == 0)
             {
                 return default;
             }
             
             return list[Random.Range(0, list.Count)];
         }
         
         public static List<T> AddRangeReturn<T>(this List<T> list, IEnumerable<T> items)
         {
             list.AddRange(items);
             return list;
         }

         public static T GetRandomElement<T>(this T[] array)
         {
             return array[Random.Range(0, array.Length)];
         }
    
         public static List<T> GetRandomElements<T>(this IEnumerable<T> list, int elementsCount) =>
             list.OrderBy(arg => Guid.NewGuid()).Take(elementsCount).ToList();
    
         public static T GetSecondToLast<T>(this List<T> list)
         {
             if (list == null || list.Count < 2)
             {
                 throw new InvalidOperationException("List does not contain enough elements.");
             }
             return list[^2];
         }

         /// <summary>
         /// For work with document data
         /// </summary>
         /// <param name="data"></param>
         /// <param name="key"></param>
         /// <param name="value"></param>
         /// <typeparam name="T"></typeparam>
         /// <returns></returns>
         public static bool SmartTryGetValue<T>(this Dictionary<string, object> data, string key, out T value)
         {
             if (data.TryGetValue(key, out var obj))
             {
                 if (obj is JObject jObject)
                 {
                     value = jObject.ToObject<T>();
                     return true;
                 }

                 if (obj is JArray jArray)
                 {
                     value = jArray.ToObject<T>();
                     return true;
                 }

                 if (obj is string json)
                 {
                     value = JsonConvert.DeserializeObject<T>(json);
                     return true;
                 }

                 if (obj == null)
                 {
                     value = default;
                     return false;
                 }

                 if (data.TryGetValue(key, out value)) return true;
                 Debug.LogError($"Cant read data. type: {data[key]?.GetType()}");
                 return false;
             }

             value = default;
             return false;
         }



         public static bool TryGetValue(this IDictionary<string, object> dictionary, string key, out int value)
         {
             if (dictionary.TryGetValue(key, out var obj))
             {
                 value = Convert.ToInt32(obj);
                 return true;
             }
             value = default;
             return false;
         }

         public static bool TryGetValue(this IDictionary<string, object> dictionary, string key, out long value)
         {
             if (dictionary.TryGetValue(key, out var obj))
             {
                 value = Convert.ToInt64(obj);
                 return true;
             }
             value = default;
             return false;
         }

         public static bool TryGetValue(this IDictionary<string, object> dictionary, string key, out float value)
         {
             if (dictionary.TryGetValue(key, out var obj))
             {
                 value = Convert.ToSingle(obj);
                 return true;
             }
             value = default;
             return false;
         }

         public static bool TryGetValue(this IDictionary<string, object> dictionary, string key, out double value)
         {
             if (dictionary.TryGetValue(key, out var obj))
             {
                 value = Convert.ToDouble(obj);
                 return true;
             }
             value = default;
             return false;
         }

         public static bool TryGetValue(this IDictionary<string, object> dictionary, string key, out string value)
         {
             if (dictionary.TryGetValue(key, out var obj))
             {
                 value = obj as string;
                 return true;
             }
             value = default;
             return false;
         }

         public static bool TryGetValue(this IDictionary<string, object> dictionary, string key, out bool value)
         {
             if (dictionary.TryGetValue(key, out var obj))
             {
                 value = Convert.ToBoolean(obj);
                 return true;
             }
             value = default;
             return false;
         }

         public static bool TryGetValue(this IDictionary<string, object> dictionary, string key, out Vector2 value)
         {
             if (dictionary.TryGetValue(key, out var obj))
             {
                 if (obj is JArray jArray)
                 {
                     var arr = jArray.ToObject<float[]>();
                     if (arr.Length == 2)
                     {
                         value = new Vector2(arr[0], arr[1]);
                         return true;
                     }
                 }
             }
             value = default;
             return false;
         }
         public static bool TryGetValue(this IDictionary<string, object> dictionary, string key, out Vector3 value)
         {
             if (dictionary.TryGetValue(key, out var obj))
             {
                 if (obj is JArray jArray)
                 {
                     var arr = jArray.ToObject<float[]>();
                     if (arr.Length == 3)
                     {
                         value = new Vector3(arr[0], arr[1], arr[2]);
                         return true;
                     }
                 }
             }
             value = default;
             return false;
         }
    
         public static bool TryGetValue<T>(this IDictionary<string, object> dictionary, string key, out T value)
         {
             if (dictionary.TryGetValue(key, out var obj))
             {
                 if (obj is long)
                     obj = Convert.ToInt32(obj);

                 if (obj is T obj1)
                 {
                     value = obj1;
                     return true;
                 }
                 Debug.LogError($"Type mismatch! Key: {key}");
             }
             value = default;
             return false;
         }
    
         public static bool TryGetValueAsVector3(this IDictionary<string, object> dictionary, string key, out Vector3 value)
         {
             if (dictionary.TryGetValue(key, out var obj))
             {
                 if (obj is JArray jArray)
                 {
                     var arr = jArray.ToObject<float[]>();
                     if (arr.Length == 3)
                     {
                         value = new Vector3(arr[0], arr[1], arr[2]);
                         return true;
                     }
                     else if (arr.Length == 2) // Default Z to 0 if only X and Y are provided
                     {
                         value = new Vector3(arr[0], arr[1], 0);
                         return true;
                     }
                 }
                 else if (obj is string str)
                 {
                     var components = str.Split(',');
                     if (components.Length == 3 &&
                         float.TryParse(components[0], out var x) &&
                         float.TryParse(components[1], out var y) &&
                         float.TryParse(components[2], out var z))
                     {
                         value = new Vector3(x, y, z);
                         return true;
                     }

                     if (components.Length == 2 && // Handle case with only X and Y, defaulting Z to 0
                         float.TryParse(components[0], out x) &&
                         float.TryParse(components[1], out y))
                     {
                         value = new Vector3(x, y, 0);
                         return true;
                     }
                 }
             }
             value = default;
             return false;
         }

         public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
         {
             if (source == null)
             {
                 return true;
             }

             return source.Any() == false;
         }
    
         public static T FindMin<T, TComp>(this IEnumerable<T> enumerable, Func<T, TComp> selector) where TComp : IComparable<TComp> =>
             Find(enumerable, selector, true);

         public static T FindMax<T, TComp>(this IEnumerable<T> enumerable, Func<T, TComp> selector) where TComp : IComparable<TComp> =>
             Find(enumerable, selector, false);
    
         public static T Next<T>(this List<T> list, T current)
         {
             if (list == null || list.Count == 0)
             {
                 return default;
             }

             int index = list.IndexOf(current);
             if (index >= 0 && index < list.Count - 1)
             {
                 return list[index + 1];
             }
             return default;
         }
    
         private static T Find<T, TComp>(IEnumerable<T> enumerable, Func<T, TComp> selector, bool selectMin) where TComp : IComparable<TComp>
         {
             if (enumerable == null)
                 return default;

             var first = true;
             var selected = default(T);
             var selectedComp = default(TComp);

             foreach (T current in enumerable)
             {
                 TComp comp = selector(current);
                 if (first)
                 {
                     first = false;
                     selected = current;
                     selectedComp = comp;
                     continue;
                 }
                 int res = selectMin
                     ? comp.CompareTo(selectedComp)
                     : selectedComp.CompareTo(comp);

                 if (res < 0)
                 {
                     selected = current;
                     selectedComp = comp;
                 }
             }
             return selected;
         }
    
         public static int FindMinValue(List<int> list)
         {
             if (list == null || list.Count == 0)
             {
                 throw new ArgumentException("List is null or empty.");
             }

             int min = list[0];
        
             for (var i = 1; i < list.Count; i++)
             {
                 if (list[i] < min)
                 {
                     min = list[i];
                 }
             }
        
             return min;
         }
    
         public static int FindMaxValue(List<int> list)
         {
             if (list == null || list.Count == 0)
             {
                 throw new ArgumentException("List is null or empty.");
             }

             int max = list[0];
        
             for (var i = 1; i < list.Count; i++)
             {
                 if (list[i] > max)
                 {
                     max = list[i];
                 }
             }
        
             return max;
         }
    
         public static void Clear<T>(this T[] array, int startIndex = 0, int length = -1)
         {
             if (array == null)
             {
                 return;
             }
             if (length < 0) length = array.Length - startIndex; 
             Array.Clear(array, startIndex, length);
         }
    
         public static List<KeyValuePair<string, int>> ToKeyValueList(this Dictionary<string, int> dictionary)
         {
             return dictionary.Select(kvp => new KeyValuePair<string, int>(kvp.Key, kvp.Value)).ToList();
         }

         public static Dictionary<string, int> ToDictionary(this List<KeyValuePair<string, int>> list)
         {
             return list.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
         }
     }
 }
