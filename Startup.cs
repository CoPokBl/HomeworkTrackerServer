using System;
using System.Collections.Generic;
using HomeworkTrackerServer.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HomeworkTrackerServer; 

public class Startup {
    public Startup(IConfiguration config) {
        Configuration = config;

        // Set storage method
        switch (Program.Config["StorageMethod"]) {
            default:
                const string error = "Invalid StorageMethod value in config, must be MySQL or RAM or File";
                Logger.Error(error);
                throw new ArgumentException(error);
                
            case "MySQL":
                Program.Storage = new MySqlStorage();
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
            throw;
        }
        Program.StorageInitialized = true;
        Logger.Info("Initialised Storage");
        
        // Load custom headers
        Logger.Info("Loading custom headers");
        List<(string, string)> headers = new ();
        string headersString = Program.Config["CustomHeaders"];
        string[] headersArray = headersString.Split(';');
        foreach (string header in headersArray) {
            string[] headerSplit = header.Split(':');
            if (headerSplit.Length != 2) {
                Logger.Error("Invalid custom header: " + header);
                continue;
            }
            headers.Add((headerSplit[0], headerSplit[1]));
            Logger.Debug("Added custom header (" + headerSplit[0] + ": " + headerSplit[1] + ")");
        }
        Program.CustomHeaders = headers.ToArray();
        Logger.Info("Loaded " + headers.Count + " custom headers");

        Logger.Debug("Finished startup");
    }

    private IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services) {
        Logger.Info("Configuring services");
            
        services.AddControllers();
        services.AddControllers().AddNewtonsoftJson();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
        Logger.Info("Configuring app");
            
        if (env.IsDevelopment()) {
            app.UseDeveloperExceptionPage();
        }

        // Only do it if our master says so
        if (Program.Config["HttpsRedirection"] == "true") {
            app.UseHttpsRedirection();
        }

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints => {
            endpoints.MapControllers();
        });
    }
}