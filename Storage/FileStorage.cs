using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using HomeworkTrackerServer.Objects;
using Newtonsoft.Json;

namespace HomeworkTrackerServer.Storage; 

public class FileStorage : RamStorage {
        
    public override void Deinit() {
        Logger.Info("Saving data to data.json...");
        try {
            File.WriteAllText("data.json", JsonConvert.SerializeObject((Users, Tasks)));
            Logger.Info("Saved data to data.json");
        }
        catch (JsonException e) {
            Logger.Error($"Failed to save data to data.json: The data failed to serialize: {e.Message}");
            Logger.Error("----- Data will not be saved -----");
        } catch (IOException e) {
            Logger.Error($"Failed to save data to data.json (IOException): {e.Message}");
            Logger.Error("Here is the data so that you can try to write it manually:");
            Logger.Error(JsonConvert.SerializeObject((Users, Tasks)));
        } catch (UnauthorizedAccessException) {
            Logger.Error($"Can't save data due to unauthorized access. Please make sure you have write access to: {Directory.GetCurrentDirectory()}");
            Logger.Error("Here is the data so that you can try to write it manually:");
            Logger.Error(JsonConvert.SerializeObject((Users, Tasks)));
        } catch (SecurityException) {
            Logger.Error($"Can't save data due to unauthorized access. Please make sure you have access to: {Directory.GetCurrentDirectory()}");
            Logger.Error("Here is the data so that you can try to write it manually:");
            Logger.Error(JsonConvert.SerializeObject((Users, Tasks)));
        }
    }

    public override void Init() {
        base.Init();
        Logger.Info("Loading data from data.json...");
        if (File.Exists("data.json")) {
            var data = JsonConvert.DeserializeObject<(Dictionary<string, User>, 
                Dictionary<string, List<Dictionary<string, string>>>)>(File.ReadAllText("data.json"));
            Users = data.Item1;
            Tasks = data.Item2;
            Logger.Info("Loaded data from data.json");
        } else {
            Logger.Info("No data.json found, creating new data.json");
            File.WriteAllText("data.json", JsonConvert.SerializeObject((Users, Tasks)));
            Logger.Info("Created new data.json");
        }
        Logger.Info("Data loaded");
            
    }
}