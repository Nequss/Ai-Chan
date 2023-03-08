﻿using Ai_Chan.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ai_Chan.Services
{
    public class ConfigurationService
    {
        public string path;

        string dataDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "data");

        public string token;
        public string prefix;

        public class Serialized
        {
            [JsonPropertyName("token")]
            public string token { get; set; }
            [JsonPropertyName("prefix")]
            public string prefix { get; set; }
        }

        public ConfigurationService()
        {
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }

            path = Path.Combine(dataDirectory, "config.json");

            if (!AssertConfigFile())
            {
                Console.WriteLine($@"Template config file created in {path} | Edit it and rerun ai-chan.");

                return;
            }
            else
            {
                Serialized serialized = JsonSerializer.Deserialize<Serialized>(File.ReadAllText(path));
                token = serialized.token;
                prefix = serialized.prefix;
            }
        }

        private bool CreateConfigTemplate()
        {
            Serialized template = new Serialized();
            template.token = "REPLACE_WITH_YOUR_BOT_TOKEN";
            template.prefix = "+";

            try
            {
                File.WriteAllText(path, JsonSerializer.Serialize<Serialized>(template));
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool AssertConfigFile()
        {
            if (!File.Exists(path))
            {
                if (CreateConfigTemplate())
                    return false;
            }
            return true;
        }
    }
}
