using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace TanteadorV4
{
    public partial class App : Application
    {
        public static NavigationPage navigationP = new NavigationPage();

        public App()
        {

            InitializeComponent();

            MainPage = navigationP;

            navigationP.PushAsync(new MainPage());

            SqlPersist.SqlPersist_Config(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SqlPersist.db3"));


        }

        /*public static SqlPersist Database
        {
            get
            {
                if (database == null)
                {
                    database = new SqlPersist(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SqlPersist.db3"));
                }
                return database;
            }
        }*/

        public void keyLeft()
        {
            
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

    }
}
