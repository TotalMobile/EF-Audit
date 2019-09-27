using Newtonsoft.Json;

namespace Phnx.Audit.EF
{
    public class JsonSerializerService : IChangeSerializerService
    {
        public JsonSerializerService()
        {
            Settings = _defaultSettings;
        }

        public JsonSerializerService(JsonSerializerSettings settings)
        {
            Settings = settings;
        }

        private static readonly JsonSerializerSettings _defaultSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            MaxDepth = 1
        };

        public JsonSerializerSettings Settings { get; set; } = _defaultSettings;

        public string Serialize(object data)
        {
            return JsonConvert.SerializeObject(data, Settings);
        }
    }
}
