using System;
using System.Collections.Generic;
using System.Drawing;

namespace HomeworkTrackerServer.Objects {
    
    public static class Converter {
        
        public static string Base64Encode(string plainText) =>
            System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(plainText));
        
        public static string Base64Decode(string base64EncodedData) => 
            System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(base64EncodedData));

        public static bool DictionaryToHomeworkTask(Dictionary<string, string> values, out HomeworkTask taskOut, bool sanitizeInputs = false) {
            string classText = "None";
            string classColour = "-1.-1.-1";
            string task = "None";
            string typeText = "None";
            string typeColour = "-1.-1.-1";
            string id = Guid.NewGuid().ToString();
            long dueDate = 0;

            if (values.ContainsKey("class")) { classText = values["class"]; }
            if (values.ContainsKey("classColour")) { classColour = values["classColour"]; }
            if (values.ContainsKey("task")) { task = values["task"]; }
            if (values.ContainsKey("type")) { typeText = values["type"]; }
            if (values.ContainsKey("typeColour")) { typeColour = values["typeColour"]; }
            if (!sanitizeInputs) { if (values.ContainsKey("id")) { id = values["id"]; } }

            try {
                if (values.ContainsKey("dueDate")) 
                    if (long.Parse(values["dueDate"]) != 0) { dueDate = DateTime.FromBinary(long.Parse(values["dueDate"])).ToBinary(); }
            
                ColorFromString(classColour);
                ColorFromString(typeColour);
            }
            catch (Exception) {
                // Invalid values
                taskOut = null;
                return false;
            }

            taskOut = new HomeworkTask {
                Class = classText,
                ClassColour = classColour,
                Type = typeText,
                TypeColour = typeColour,
                Task = task,
                Id = id,
                DueDate = dueDate
            };
            return true;
        }
        
        public static Color ColorFromString(string str) {
            if (str == "-1.-1.-1") { return Color.Empty; }
            string[] strs = str.Split(".");
            return Color.FromArgb(255, Convert.ToInt32(strs[0]), Convert.ToInt32(strs[1]), Convert.ToInt32(strs[2]));
        }

    }
    
}