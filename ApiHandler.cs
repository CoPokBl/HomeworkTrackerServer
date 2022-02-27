using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace HomeworkTrackerServer {
    public static class ApiHandler {
        
        public static string Handle(HttpListenerRequest req, string reqText, out int status) {
            
            // Get auth
            bool authenticated;
            string username;
            try {
                string token = req.Headers["x-api-token"].Replace("!", ".");
                // do magic
                authenticated = TokenHandler.ValidateCurrentToken(token, out username);
            }
            catch (Exception e) {
                // header isn't there
                Program.Debug(e.ToString());
                username = "";
                authenticated = false;
            }
            
            Dictionary<string, string> requestContent;
            status = 400;
                    
            try { requestContent = JsonConvert.DeserializeObject<Dictionary<string, string>>(reqText); }
            catch (Exception) /* not valid request */ { return "Invalid Request, malformed JSON"; }

            if (requestContent == null)                     return "Invalid Request, request content is null";
            if (!requestContent.ContainsKey("requestType")) return "Invalid Request, your request must contain the 'requestType' property";
            string failResponse;
            
            switch (requestContent["requestType"]) {
                
                default:
                    return "Invalid requestType value";
                
                case "login":
                    if (!ValidArgs(new [] {
                            "username", 
                            "password"
                        }, requestContent, out failResponse)) { return failResponse; }

                    if (Program.Storage.AuthUser(requestContent["username"], requestContent["password"])) {
                        // login success
                        status = 200;
                        return TokenHandler.GenerateToken(requestContent["username"]);  // Give em da token
                    }
                    status = 403;
                    return "Invalid Username/Password";
                
                case "register":
                    if (!ValidArgs(new [] {
                            "username", 
                            "password"
                        }, requestContent, out failResponse)) { return failResponse; }
                    
                    // Created user
                    if (requestContent["username"].Length > 64) return "Username cannot be longer than 64 characters";

                    if (!Program.Storage.CreateUser(
                            requestContent["username"], requestContent["password"])) {
                        status = 409;
                        return "Username taken";
                    }

                    status = 200;
                    return "Success";
                
                case "changePassword":
                    
                    if (!ValidArgs(new [] {
                            "password"
                        }, requestContent, out failResponse)) { return failResponse; }
                    
                    if (!authenticated) {
                        status = 403;
                        return "Invalid Token";
                    }
                    
                    Program.Storage.ChangePassword(username, requestContent["password"]);

                    status = 200;
                    return "Success";
                
                case "deleteAccount":

                    if (!authenticated) {
                        status = 403;
                        return "Invalid Token";
                    }

                    // Remove it
                    Program.Storage.RemoveUser(username);
                    
                    // Cool
                    status = 200;
                    return "Success";
                
                case "getTasks":
                    
                    if (!authenticated) {
                        status = 403;
                        return "Invalid Token";
                    }

                    var items = Program.Storage.GetTasks(username);
                    status = 200;
                    return JsonConvert.SerializeObject(items);
                
                case "addTask":

                    if (!authenticated) {
                        status = 403;
                        return "Invalid Token";
                    }

                    try {
                        Program.Storage.AddTask(username, requestContent);
                    }
                    catch (Exception e) {
                        return $"A value provided in your request was invalid: {e.Message}";
                    }

                    status = 200;
                    return "Success";
                
                case "removeTask":
                    if (!ValidArgs(new [] {
                            "id"
                        }, requestContent, out failResponse)) { return failResponse; }

                    if (!authenticated) {
                        status = 403;
                        return "Invalid Token";
                    }

                    if (!Guid.TryParse(requestContent["id"], out Guid rmid)) {
                        return "Task ID isn't a valid GUID";
                    }

                    if (!Program.Storage.RemoveTask(username, rmid.ToString())) {
                        // It failed
                        return "Invalid task ID, that task doesn't exist";
                    }
                    
                    // it worked and deleted the task
                    status = 200;
                    return "Success";
                
                case "editTask":
                    if (!ValidArgs(new [] {
                            "id",
                            "field",
                            "value"
                        }, requestContent, out failResponse)) { return failResponse; }

                    if (!authenticated) {
                        status = 403;
                        return "Invalid Token";
                    }

                    if (!Guid.TryParse(requestContent["id"], out Guid eid)) {
                        return "Task ID isn't a valid GUID";
                    }

                    try {
                        if (!Program.Storage.EditTask(username, eid.ToString(), 
                                requestContent["field"], requestContent["value"])) {
                            // It failed
                            return "Invalid task ID, that task doesn't exist";
                        }
                    }
                    catch (Exception e) {
                        return "Either the field or value was invalid: " + e.Message;
                    }

                    // it worked and edited the task
                    status = 200;
                    return "Success";

                case "checkLogin":
                    if (authenticated) {
                        status = 200;
                        return "Valid Token";
                    }
                    status = 403;
                    return "Invalid Token";

                case "ping":
                    status = 200;
                    return "pong!";
                
                case "getVersion":
                    status = 200;
                    return Program.Ver.ToString();
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