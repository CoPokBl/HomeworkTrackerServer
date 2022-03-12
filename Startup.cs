using System;
using HomeworkTrackerServer.Objects;
using HomeworkTrackerServer.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HomeworkTrackerServer {
    public class Startup {
        public Startup(IConfiguration config) {
            Configuration = config;

            Program.LoggingLevel = int.Parse(config["LoggingLevel"]);
            
            // Init token handler
            Program.TokenHandler = new TokenHandler(config);

            // Set storage method
            switch (config["StorageMethod"]) {
                default:
                    Program.Error("Invalid StorageMethod value in config, must be MySQL or RAM");
                    throw new ArgumentException("Invalid StorageMethod value in config, must be MySQL or RAM");
                
                case "MySQL":
                    Program.Storage = new MySQLStorage();
                    break;
                
                case "RAM":
                    Program.Info("Warning: StorageMethod is set to RAM, data will be deleted upon restart");
                    Program.Storage = new RamStorage();
                    break;
            }

            Program.Info("Initialising Storage");
            try {
                Program.Storage.Init(config);
            }
            catch (Exception e) {
                Program.Error("Failed to initialize storage: " + e.Message);
                Program.Debug(e.ToString());  // Debug whole error
                throw new Exception("Failed to init storage");
            }
            Program.Info("Initialised Storage");
            
            Program.Debug("Finished startup");
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
