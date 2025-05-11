using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnrealLocres.Converter
{
    public sealed class JsonConverter : BaseConverter
    {

        public override string ExportExtension => "json";

        public override string ImportExtension => "json";

        private string GetJsonGroupName(string logResKey)
        {
            // Extract the part before /
            var index = logResKey.IndexOf('/');
            // If there is no /, it means there is no jsonGroupName, use # to represent it
            if (index == -1) {
                return "#";
            }
            string groupName = logResKey.Substring(0, index);
            // Remove the prefix ST_
            if (groupName.StartsWith("ST_")) {
                groupName = groupName.Substring(3);
            } else {
                // If there is no ST_, add # to mark it
                groupName = "#" + groupName;
            }
            return groupName;
        }

        private string GetJsonKey(string logResKey)
        {
            // Extract the part after /
            var index = logResKey.IndexOf('/');
            if (index == -1) {
                return logResKey;
            }
            return logResKey.Substring(index + 1);
        }

        private string GetLocResKey(string jsonGroupName, string jsonKey)
        {
            // jsonGroupName being # means there is no jsonGroupName, directly return jsonKey
            if (jsonGroupName == "#") {
                return jsonKey;
            }
            // If jsonGroupName starts with #, remove #, otherwise add ST_
            if (jsonGroupName.StartsWith("#")) {
                jsonGroupName = jsonGroupName.Substring(1);
            } else {
                jsonGroupName = "ST_" + jsonGroupName;
            }
            return jsonGroupName + "/" + jsonKey;
        }

        protected override List<TranslationEntry> Read(TextReader reader)
        {
            var result = new List<TranslationEntry>();

            var json = reader.ReadToEnd();
            var jsonObj = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);

            foreach (var jsonGroupName in jsonObj.Keys) {
                foreach (var item in jsonObj[jsonGroupName]) {
                    var logResKey = GetLocResKey(jsonGroupName, item.Key);
                    result.Add(new TranslationEntry(logResKey, "", item.Value));
                }
            }

            return result;
        }

        protected override void Write(List<TranslationEntry> data, TextWriter writer)
        {
            JObject obj = new JObject();
            foreach (var entry in data) {
                var jsonGroupName = GetJsonGroupName(entry.Key);
                var jsonKey = GetJsonKey(entry.Key);
                if (!obj.ContainsKey(jsonGroupName)) {
                    obj[jsonGroupName] = new JObject();
                }
                obj[jsonGroupName][jsonKey] = entry.Source;
            }
            writer.Write(obj);
        }
    }
}
