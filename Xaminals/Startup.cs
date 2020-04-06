using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xaminals.Services;

namespace Xaminals
{
    /// <summary>
    /// This all came from https://montemagno.com/add-asp-net-cores-dependency-injection-into-xamarin-apps-with-hostbuilder/
    /// </summary>
    public static class Startup
    {
        public static IServiceProvider ServiceProvider { get; set; }


        public static void Init()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Xaminals.appsettings.json"))
            {
                var host = new HostBuilder()
                    .ConfigureHostConfiguration(c =>
                    {
                        // Tell the host configuration where to file the file (this is required for Xamarin apps)
                        c.AddCommandLine(new string[] { $"ContentRoot={FileSystem.AppDataDirectory}" });

                        //read in the configuration file!
                        c.AddJsonStream(stream);
                    })
                    .ConfigureServices((c, x) =>
                    {
                        // Configure our local services and access the host configuration
                        ConfigureServices(c, x);
                    })
                    .ConfigureLogging(l => l.AddConsole(o =>
                    {
                        //setup a console logger and disable colors since they don't have any colors in VS
                        o.DisableColors = true;
                    }))
                    .Build();

                //Save our service provider so we can use it later.
                ServiceProvider = host.Services;
            }
        }

        private static void ConfigureServices(HostBuilderContext ctx, IServiceCollection services)
        {
            // The HostingEnvironment comes from the appsetting.json and could be optionally used to configure the mock service
            if (ctx.HostingEnvironment.IsDevelopment())
            {
                // add as a singleton so only one ever will be created.
                services.AddSingleton<IDataService, MockDataService>();
            }
            else
            {
                services.AddSingleton<IDataService, MyDataService>();
            }

            services.AddTransient<ISampleService, RealSampleService>();



            // add the ViewModel, but as a Transient, which means it will create a new one each time.
            services.AddTransient<MyViewModel>();

            //Another thing we can do is access variables from that json file
            var world = ctx.Configuration["Hello"];
        }



        public interface IDataService
        {
            void DoStuff();
        }

        public class MyDataService : IDataService
        {
            // We need access to the ILogger from Microsoft.Extensions so pass it into the constructor
            ILogger<MyDataService> logger;
            public MyDataService(ILogger<MyDataService> logger)
            {
                this.logger = logger;
            }

            public void DoStuff()
            {
                logger.LogCritical("You just called DoStuff from MyDataService");
            }
        }

        public class MockDataService : IDataService
        {
            // We need access to the ILogger from Microsoft.Extensions so pass it into the constructor
            ILogger<MyDataService> logger;
            public MockDataService(ILogger<MyDataService> logger)
            {
                this.logger = logger;
            }

            public void DoStuff()
            {
                logger.LogCritical("You just called DoStuff from MockDataService");
            }
        }

        public class MyViewModel : INotifyPropertyChanged
        {
            private readonly IDataService dataService;
            private readonly ISampleService sampleService;

            public MyViewModel(IDataService dataService, ISampleService sampleService)
            {
                this.dataService = dataService;
                this.sampleService = sampleService;
                DoItCommand = new Command(() => MyText = $"2 + 2 = {sampleService.Add(2, 2)}");
            }

            public void DoIt()
            {
                dataService.DoStuff();
            }

            public Command DoItCommand { get; private set; }

            private string myText;

            public string MyText
            {
                get => myText;
                set { SetField(ref myText, value); }
            }


            #region INotifyPropertyChanged Implementation
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
            {
                if (EqualityComparer<T>.Default.Equals(field, value))
                    return false;
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }
            #endregion

        }
    }
}