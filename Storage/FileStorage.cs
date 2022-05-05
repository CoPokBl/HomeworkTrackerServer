using System.Collections.Generic;
using System.IO;
using HomeworkTrackerServer.Objects;
using Newtonsoft.Json;
using RayKeys.Misc;

namespace HomeworkTrackerServer.Storage {
    public class FileStorage : RamStorage {
        
        public override void Deinit() {
            Logger.Info("Saving data to data.json...");
            File.WriteAllText("data.json", JsonConvert.SerializeObject((Users, Tasks)));
            Logger.Info("Saved data to data.json");
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
}
