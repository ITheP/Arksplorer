using Arksplorer.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Arksplorer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Settings.Default.Save();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Note, during development am happy for exceptions to stop code from running. At run time, prefer to log what's happened and attempt
            // to continue execution if at all possible.
#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += (s, ex) => LogUnhandledException((Exception)ex.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");
            DispatcherUnhandledException += (s, ex) => LogUnhandledException(ex.Exception, "Application.Current.DispatcherUnhandledException");
            TaskScheduler.UnobservedTaskException += (s, ex) => LogUnhandledException(ex.Exception, "TaskScheduler.UnobservedTaskException");
#endif

            Window splashScreen = new SplashScreen();
            this.MainWindow = splashScreen;
            splashScreen.Show();

            // Run startup code in different thread so UI still updates
            // but we wait here for it to be complete!
            // This is for future expansion should startup requirements grow.
            MainWindow mainWindow = null;
            Task.Run(() =>
            {
                // ToDo: Init any startup stuff async. here

                this.Dispatcher.Invoke(() =>
                {
                    mainWindow = new MainWindow();  // ToDo: Init any startup stuff async.
                    Globals.MainWindow = mainWindow;
                    this.MainWindow = mainWindow;
                    splashScreen.Close();
                    mainWindow.Show();
                });
            });
        }

        /// <summary>
        /// Saves off exception information in to a log file. Only used in releases, not when run in a debug development state
        /// </summary>
        /// <param name="exception">The exception<see cref="Exception"/></param>
        /// <param name="@event">The event<see cref="string"/></param>
        private static void LogUnhandledException(Exception exception, string @event)
        {
            MessageBox.Show($"Arksplorer generated a critical exception. For debugging purposes, it will attempt to dump details of this in {Globals.CrashFile}. We will now do our best to continue!", "Critical error");

            string result = $"Arksplorer experienced an itsy bitsy problem...{Environment.NewLine}{Environment.NewLine}Exception:{Environment.NewLine}{exception.Message}";

            if (exception.InnerException != null)
                result += $"{Environment.NewLine}{Environment.NewLine}Inner Exception:{Environment.NewLine}{exception.InnerException}";

            if (exception.StackTrace != null)
                result += $"{Environment.NewLine}{Environment.NewLine}Stack Trace:{Environment.NewLine}{exception.StackTrace}";

            Globals.AddToCrashFile(result);
        }
    }
}
