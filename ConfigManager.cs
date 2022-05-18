using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace HomeworkTrackerServer; 

public static class ConfigManager {
        
    private const string ConfigFileName = "config.json";  // I think this is a pretty good name for a config file.
        
    // The default values for the config file
    private static readonly Dictionary<string, string> DefaultConfig = new Dictionary<string, string> {
        { "bind_address", "http://*:9898" },
        { "mysql_ip", "mysql.example.net" },
        { "mysql_user", "admin" },
        { "mysql_password", "password" },
        { "mysql_database", "homework_tracker" },
        { "LoggingLevel", "3" },
        { "StorageMethod", "RAM" },
        { "TokenSecret", "secret (CHANGE THIS)" },
        { "TokenExpirationHours", "168" },
        { "TokenIssuer", "HomeworkTracker" },
        { "TokenAudience", "HomeworkTrackerUsers" }
    };
        
    // All the values that should be in the config file
    private static string[] RequiredConfigValues => DefaultConfig.Keys.ToArray();

    /// <summary>
    /// Loads the config file and returns a dictionary of the values
    /// </summary>
    /// <returns>The config file represented as a Dictionary</returns>
    public static Dictionary<string, string> LoadConfig() {
            
        // Don't bother creating a default config because they get it when they build
        if (!File.Exists(ConfigFileName)) {
            // It doesn't exist, so create it and give them the default config
            File.Create(ConfigFileName).Close();
            File.WriteAllText(ConfigFileName, JsonConvert.SerializeObject(DefaultConfig));
            Logger.Info("Config file created with default values");
            return DefaultConfig;
        }
        // Get config data
        string data = File.ReadAllText(ConfigFileName);

        Dictionary<string, string> configDict;
        try {
            configDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
        }
        catch (JsonSerializationException e) {
            // Config is invalid
            throw new Exception("Config file is invalid: " + e.Message);
        }
            
        // Check if all the required values are there
        bool wholeConfigValid = true;
        foreach (string requiredValue in RequiredConfigValues) {
            if (configDict.ContainsKey(requiredValue)) continue;
            // Missing a required value, so add it
            configDict.Add(requiredValue, DefaultConfig[requiredValue]);
            Logger.Info($"Config file is missing required value ({requiredValue}) and was added with " +
                        $"default value ({DefaultConfig[requiredValue]})");
            wholeConfigValid = false;
        }
        if (!wholeConfigValid) {
            // Save the config file
            File.WriteAllText(ConfigFileName, JsonConvert.SerializeObject(configDict));
            Logger.Info("Wrote missing config values to config file");
        }
            
        // Return the patched config
        return configDict;
            
    }

}