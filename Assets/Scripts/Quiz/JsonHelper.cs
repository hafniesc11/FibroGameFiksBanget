using System;
using System.Collections.Generic;
using UnityEngine;

namespace FOPC.Quiz
{
    /// <summary>
    /// Utility untuk melakukan serialisasi & deserialisasi JSON Array pada Unity JsonUtility.
    /// </summary>
    public static class JsonHelper
    {
        [Serializable]
        private class Wrapper<T>
        {
            public T[] items;
        }

        /// <summary>
        /// Mengubah string JSON Array "[ {...}, {...} ]" menjadi array object T[]
        /// </summary>
        public static T[] FromJsonArray<T>(string json)
        {
            if (string.IsNullOrEmpty(json) || json.Trim() == "[]")
            {
                return new T[0];
            }

            string newJson = "{\"items\":" + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper != null && wrapper.items != null ? wrapper.items : new T[0];
        }

        /// <summary>
        /// Mengubah string JSON Array "[ {...}, {...} ]" menjadi List<T>
        /// </summary>
        public static List<T> FromJsonList<T>(string json)
        {
            T[] array = FromJsonArray<T>(json);
            return new List<T>(array);
        }

        /// <summary>
        /// Mengubah array object T[] menjadi string JSON Array
        /// </summary>
        public static string ToJsonArray<T>(T[] array)
        {
            Wrapper<T> wrapper = new Wrapper<T> { items = array };
            string json = JsonUtility.ToJson(wrapper);
            int startIndex = json.IndexOf('[');
            int endIndex = json.LastIndexOf(']');
            if (startIndex >= 0 && endIndex >= startIndex)
            {
                return json.Substring(startIndex, endIndex - startIndex + 1);
            }
            return "[]";
        }
    }
}
