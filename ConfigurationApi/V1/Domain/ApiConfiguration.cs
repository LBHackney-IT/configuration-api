using System.Collections.Generic;
using Newtonsoft.Json;

namespace ConfigurationApi.V1.Domain
{
    public class ApiConfiguration
    {
        public string Type { get; set; }

        public Configuration Configuration { get; set; }

        public Dictionary<string, object> FeatureToggles { get; set; }

        public static ApiConfiguration Create(string json)
        {
            return JsonConvert.DeserializeObject<ApiConfiguration>(json);
        }
    }
}
