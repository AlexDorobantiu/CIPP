using Newtonsoft.Json;
using System.IO;

namespace CIPP.Utils
{
    static class JsonUtil
    {
        public static void SerializeToFile(string path, object obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            System.IO.File.WriteAllText(path, json);
        }

        public static T DeserializeFromFile<T>(string path)
        {
            if (File.Exists(path))
            {
                string json = System.IO.File.ReadAllText(path);
                return JsonConvert.DeserializeObject<T>(json);
            }
            return default(T);
        }
    }
}
