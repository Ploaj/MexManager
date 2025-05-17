using System.Text.Json;

namespace mexLib.Utilties
{
    public static class MexJsonSerializer
    {
        private static readonly JsonSerializerOptions _serializeoptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            //ReferenceHandler = ReferenceHandler.Preserve,
        };
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="f"></param>
        /// <returns></returns>
        public static string Serialize<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, _serializeoptions);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="f"></param>
        /// <returns></returns>
        public static T? Deserialize<T>(string filePath)
        {
            return JsonSerializer.Deserialize<T>(File.ReadAllText(filePath), _serializeoptions);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="f"></param>
        /// <returns></returns>
        public static T? Deserialize<T>(byte[] data)
        {
            return JsonSerializer.Deserialize<T>(data, _serializeoptions);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="projectPath"></param>
        /// <param name="relativePath"></param>
        /// <param name="assign"></param>
        public static void LoadData<T>(string filePath, Action<T> assign)
        {
            T? data = JsonSerializer.Deserialize<T>(File.ReadAllText(filePath), _serializeoptions);

            if (data != null)
            {
                assign(data);
            }
        }
    }
}
