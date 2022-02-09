using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using Newtonsoft.Json;

namespace HomeworkTrackerServer {
    public static class ApiHandler {
        
        public static string Handle(HttpListenerRequest req, string reqText, out int status) {
            Dictionary<string, string> requestContent;
            status = 400;
                    
            try {
                requestContent = JsonConvert.DeserializeObject<Dictionary<string, string>>(reqText);
            }
            catch (Exception) {
                // not valid request
                return "Invalid Request";
            }

            if (requestContent == null) return "Invalid Request";
            if (!requestContent.ContainsKey("requestType")) return "Invalid Request";
            
            switch (requestContent["requestType"]) {
                
                default:
                    return "Invalid Request";
                
                case "register":
                    if (!requestContent.ContainsKey("username")) return "Invalid Request";
                    if (!requestContent.ContainsKey("password")) return "Invalid Request";
                    // Created user
                    if (!Program.Storage.CreateUser(
                            requestContent["username"], requestContent["password"])) {
                        status = 409;
                        return "Username taken";
                    }

                    status = 200;
                    return "Success";
                
                case "getTasks":
                    if (!requestContent.ContainsKey("username")) return "Invalid Request";
                    if (!requestContent.ContainsKey("password")) return "Invalid Request";
                    if (!Program.Storage.AuthUser(requestContent["username"], requestContent["password"])) {
                        status = 403;
                        return "Auth failed";
                    }

                    var items = Program.Storage.GetTasks(requestContent["username"]);
                    status = 200;
                    return JsonConvert.SerializeObject(items);
                
                case "addTask":
                    if (!requestContent.ContainsKey("username")) return "Invalid Request";
                    if (!requestContent.ContainsKey("password")) return "Invalid Request";
                    if (!requestContent.ContainsKey("class")) return "Invalid Request";
                    if (!requestContent.ContainsKey("task")) return "Invalid Request";
                    if (!requestContent.ContainsKey("taskColour")) return "Invalid Request";
                    if (!requestContent.ContainsKey("type")) return "Invalid Request";
                    if (!requestContent.ContainsKey("typeColour")) return "Invalid Request";
                    
                    if (!Program.Storage.AuthUser(requestContent["username"], requestContent["password"])) {
                        status = 403;
                        return "Auth failed";
                    }

                    try {
                        Program.Storage.AddTask(requestContent["username"], requestContent["class"], 
                            new ColouredString(requestContent["task"], Color.FromName(requestContent["taskColour"])), 
                            new ColouredString(requestContent["type"], Color.FromName(requestContent["typeColour"])));
                    }
                    catch (Exception) {
                        return "A colour provided in your request was invalid";
                    }

                    status = 200;
                    return "Success";
                
                case "checkLogin":
                    if (!requestContent.ContainsKey("username")) return "Invalid Request";
                    if (!requestContent.ContainsKey("password")) return "Invalid Request";
                    Program.Debug(requestContent["password"]);
                    if (Program.Storage.AuthUser(requestContent["username"], requestContent["password"])) {
                        status = 200;
                        return "Authentication Successful";
                    }

                    status = 403;
                    return "Authentication Failed";
            }
            
        }
    }
    
}