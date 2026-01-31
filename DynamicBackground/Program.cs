using System;
using System.Windows.Forms;
using DynamicBackground.Infrastructure;

namespace DynamicBackground
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Initialize DI container
                var serviceProvider = AppBootstrapper.CreateServiceProvider();

                // Run application with DI
                Application.Run(new DynamicBackgroundUI(serviceProvider));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application initialization failed: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
