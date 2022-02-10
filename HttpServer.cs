using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using JsonConverter = Newtonsoft.Json.JsonConverter;

namespace HomeworkTrackerServer {
    public static class HttpServer {
        private const bool RunServer = true;
        private static int _requests;
        private static HttpListener _listener;
        
        // Options
        private static readonly string[] BindIPs = {
            "http://*:9898/"
        };
        
        public static async Task Start() {

            Program.Info("Binding IPs");
            _listener = new HttpListener();
            foreach (string ip in BindIPs) {
                Program.Debug($"Binding IP: {ip}");
                _listener.Prefixes.Add(ip);
            }
            _listener.Start();
            Program.Info("Listening for connections...");
            
            while (RunServer) {
                // Will wait here until we hear from a connection
                var ctx = await _listener.GetContextAsync();

                // Peel out the requests and response objects
                var req = ctx.Request;
                var resp = ctx.Response;

                string serve;
                int status;

                // Print out some info about the request to debug
                Program.Debug($"Request #{++_requests} from '{req.UserHostName}' on '{req.UserAgent}' using '{req.HttpMethod}'");

                if (req.Url.AbsolutePath.StartsWith("/api")) {
                    
                    string strmContents = GetText(req.InputStream, req.ContentLength64);
                    
                    if (strmContents.Length > 1000) {
                        status = 400;
                        serve = "Request body cannot contain more than 1000 characters";
                    } else {
                        try {
                            serve = ApiHandler.Handle(req, strmContents, out status);
                        }
                        catch (Exception e) {
                            Program.Error(e.ToString());
                            status = 500;
                            serve = "Internal Server Error";
                        }
                    }

                }
                else {
                    // not an API request
                    Program.Debug($"Serving homepage to {req.UserHostName}");
                    serve = await File.ReadAllTextAsync("Homepage.html");
                    status = 200;
                }

                // Write the response info
                byte[] data = Encoding.UTF8.GetBytes(serve);
                resp.ContentType = "text/html";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;
                resp.StatusCode = status;

                // Write out to the response stream (asynchronously), then close it
                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();
            }
        }


        private static string GetText(Stream stream, long streamLength) {
            int strRead;  // cbyte
            int strLen = (int)streamLength;  // Find number of bytes in stream.
            byte[] strArr = new byte[strLen];  // Create a byte array.
            strRead = stream.Read(strArr, 0, strLen);  // Read stream into byte array.
            return Encoding.Default.GetString(strArr);  // Return the string version
        }
    }
}