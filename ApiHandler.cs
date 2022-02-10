using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
                return "Invalid Request, failed to convert request string to a dictionary";
            }

            if (requestContent == null) return "Invalid Request, request content is null";
            if (!requestContent.ContainsKey("requestType")) return "Invalid Request, your request must contain the 'requestType' property";
            string failResponse;
            
            switch (requestContent["requestType"]) {
                
                default:
                    return "Invalid requestType value";
                
                case "register":
                    if (!ValidArgs(new [] {
                            "username", 
                            "password"
                        }, requestContent, out failResponse)) { return failResponse; }
                    
                    // Created user
                    if (requestContent["username"].Length > 20) return "Username cannot be longer than 20 characters";
                    if (requestContent["password"].Length > 64) return "Password must be a 64 character (256 bit) SHA256 hash";
                    if (!Program.Storage.CreateUser(
                                requestContent["username"], requestContent["password"])) {
                        status = 409;
                        return "Username taken";
                    }

                    status = 200;
                    return "Success";
                
                case "getTasks":
                    if (!ValidArgs(new [] {
                            "username", 
                            "password"
                        }, requestContent, out failResponse)) { return failResponse; }
                    
                    if (!Program.Storage.AuthUser(requestContent["username"], requestContent["password"])) {
                        status = 403;
                        return "Auth failed";
                    }

                    var items = Program.Storage.GetTasks(requestContent["username"]);
                    status = 200;
                    return JsonConvert.SerializeObject(items);
                
                case "addTask":
                    if (!ValidArgs(new [] {
                            "username", 
                            "password", 
                            "class", 
                            "classColour", 
                            "task", 
                            "type", 
                            "typeColour"
                        }, requestContent, out failResponse)) { return failResponse; }

                    if (!Program.Storage.AuthUser(requestContent["username"], requestContent["password"])) {
                        status = 403;
                        return "Auth failed";
                    }

                    try {
                        Program.Storage.AddTask(requestContent["username"], 
                            new ColouredString(requestContent["class"], Color.FromName(requestContent["classColour"])), 
                            requestContent["task"], 
                            new ColouredString(requestContent["type"], Color.FromName(requestContent["typeColour"])));
                    }
                    catch (Exception) {
                        return "A colour provided in your request was invalid";
                    }

                    status = 200;
                    return "Success";
                
                case "checkLogin":
                    if (!ValidArgs(new[] {
                            "username", 
                            "password"
                        }, requestContent, out failResponse)) { return failResponse; }
                    
                    if (Program.Storage.AuthUser(requestContent["username"], requestContent["password"])) {
                        status = 200;
                        return "Authentication Successful";
                    }

                    status = 403;
                    return "Authentication Failed";
            }
            
        }

        private static bool ValidArgs(string[] requiredArgs, Dictionary<string, string> request, out string failReason) {
            failReason = "";

            foreach (var arg in requiredArgs) {
                if (request.Keys.Contains(arg)) continue;
                failReason = "You must provide the '" + arg + "' value in your request";
                return false;
            }

            return true;
        }
        
    }
    
}