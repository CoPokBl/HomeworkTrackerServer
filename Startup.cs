using System;
using System.Collections.Generic;
using System.IO;
using HomeworkTrackerServer.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RayKeys.Misc;

namespace HomeworkTrackerServer {
    public class Startup {
        public Startup(IConfiguration config) {
            Configuration = config;
            
            // Custom config because ASP.NETs config system is stupid
            try {
                // Don't bother creating a default config because they get it when they build
                if (!File.Exists("config.json")) {
                    throw new FileNotFoundException("config.json not found, please create it or rebuild the project");
                }
                // Get config data
                string data = File.ReadAllText("config.json");
                Dictionary<string, string> configDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
                Program.Config = configDict;
                Logger.Info("Loaded config");
            }
            catch (Exception e) {
                // Failed to load config
                Logger.Error($"Failed to load config {e.Message}");
                throw new Exception("Failed to load config");
            }

            Program.LoggingLevel = (LogLevel) int.Parse(Program.Config["LoggingLevel"]);
            Logger.Init(Program.LoggingLevel);

            // Set storage method
            switch (Program.Config["StorageMethod"]) {
                default:
                    Logger.Error("Invalid StorageMethod value in config, must be MySQL or RAM");
                    throw new ArgumentException("Invalid StorageMethod value in config, must be MySQL or RAM");
                
                case "MySQL":
                    Program.Storage = new MySQLStorage();
                    break;
                
                case "RAM":
                    Logger.Info("Warning: StorageMethod is set to RAM, data will be deleted upon restart");
                    Program.Storage = new RamStorage();
                    break;
                
                case "File":
                    Program.Storage = new FileStorage();
                    break;
            }

            Logger.Info("Initialising Storage");
            try {
                Program.Storage.Init();
            }
            catch (Exception e) {
                Logger.Error("Failed to initialize storage: " + e.Message);
                Logger.Debug(e.ToString());  // Debug whole error
                throw new Exception("Failed to init storage");
            }
            Logger.Info("Initialised Storage");
            
            Logger.Debug("Finished startup");
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddControllers();
            services.AddControllers().AddNewtonsoftJson();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            // TODO: Add HTTPS support
            // app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
