using System;
using HomeworkTrackerServer.Objects;
using HomeworkTrackerServer.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RayKeys.Misc;

namespace HomeworkTrackerServer {
    public class Startup {
        public Startup(IConfiguration config) {
            Configuration = config;

            Program.LoggingLevel = (LogLevel) int.Parse(config["LoggingLevel"]);
            Logger.Init(Program.LoggingLevel);
            
            // Init token handler
            Program.TokenHandler = new TokenHandler(config);

            // Set storage method
            switch (config["StorageMethod"]) {
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
            }

            Logger.Info("Initialising Storage");
            try {
                Program.Storage.Init(config);
            }
            catch (Exception e) {
                Logger.Error("Failed to initialize storage: " + e.Message);
                Logger.Debug(e.ToString());  // Debug whole error
                throw new Exception("Failed to init storage");
            }
            Logger.Info("Initialised Storage");
            
            Logger.Debug("Finished startup");
        }

        public IConfiguration Configuration { get; }

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

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
