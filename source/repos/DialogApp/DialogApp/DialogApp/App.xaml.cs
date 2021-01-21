using DialogApp.Interfaces;
using DialogApp.Models;
using MySqlConnector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
using Xamarin.Essentials;
using Xamarin.Forms;
using PermissionStatus = Plugin.Permissions.Abstractions.PermissionStatus;

namespace DialogApp
{
    public partial class App : Application
    {

        public static string UGUID = Preferences.Get("GUID", "");
        public static string newUser1 = Guid.NewGuid().ToString();

        //test

        [Obsolete]
        public App()
        {
            InitializeComponent();

            MySqlConnection con = new MySqlConnection("Server=kep05.vas-server.cz;database=aplication_2;User Id=aplication.2;Password=pR5FCYgzIwZZlqMQp");
            con.Open();

            MySqlCommand cmd = con.CreateCommand();
            cmd.CommandText = "SELECT * FROM user_data";
            MySqlDataReader reader = cmd.ExecuteReader();


            while (reader.Read())
            {
                if (reader.GetString(0) == Preferences.Get("GUID", "ERROR"))
                {
                    if ((reader.GetString(2) != "") && (reader.GetString(3) != ""))
                    {
                        MainPage = new SetedMainPage();
                    }
                    else
                    {
                        MainPage = new MainPage();
                    }
                }


               // MainPage = new MainPage();
            }
        }

        protected override void OnStart()
        {

            var current = Connectivity.NetworkAccess;


            if (current == NetworkAccess.Internet)
            {

                MySqlConnection con = new MySqlConnection("Server=kep05.vas-server.cz;database=aplication_2;User Id=aplication.2;Password=pR5FCYgzIwZZlqMQp");
                con.Open();

                MySqlCommand cmd = con.CreateCommand();

                if (!Application.Current.Properties.ContainsKey("FirstUse"))
                {

                    cmd.CommandText = "INSERT INTO user_data(user_data_id) VALUES('" + newUser1 + "')";
                    cmd.ExecuteNonQuery();

                    if (!Preferences.ContainsKey("GUID")) Preferences.Set("GUID", newUser1);

                    UGUID = Preferences.Get("GUID", "null");

                    Application.Current.Properties["FirstUse"] = false;

                }

            }
        }

        [Obsolete]
#pragma warning disable CS0809
        protected override void OnSleep()
#pragma warning restore CS0809
        {

            Root r = new Root();

            MySqlConnection con = new MySqlConnection("Server=kep05.vas-server.cz;database=aplication_2;User Id=aplication.2;Password=pR5FCYgzIwZZlqMQp");
            con.Open();

            MySqlCommand cmd = con.CreateCommand();
            cmd.CommandText = "SELECT * FROM user_data";
            MySqlDataReader reader = cmd.ExecuteReader();

            DateTime user = DateTime.Now;

            while (reader.Read())
            {
                if (reader.GetString(0) == Preferences.Get("GUID", "ERROR"))
                {

                    //uživatel nalezen
                    user = DateTime.ParseExact(reader[1].ToString(), "HH:mm:ss", CultureInfo.InvariantCulture);
                    r.ID = reader.GetString(2);
                }
            }

            async void LoadCallLog()
            {

                var statusContact = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Contacts);
                var statusPhone = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Phone);
                var statusStorage = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);

                if (statusContact == PermissionStatus.Granted && statusPhone == PermissionStatus.Granted)
                {
                    var Logg = DependencyService.Get<ICallLog>().GetCallLogs();

                   // root.ID = Preferences.Get("GUID", "ERROR");
                    r.calls = (List<CallLogModel>)Logg;

                    var serialized = JsonConvert.SerializeObject(r);

                    using (var client = new HttpClient())
                    {

                        await client.PostAsync("https://intranet.ryzi-okna.cz/api/ZtW2@R/export-phones", new StringContent(serialized));
                        // await client.PostAsync("https://webhook.site/87a500dd-566e-4842-bc8b-430baf8f632a", new StringContent(serialized));
                    }
                }
            }


            var seconds = TimeSpan.FromSeconds(1);
            Device.StartTimer(seconds, () => {

                //pokud čas odesílání dat uživatele == teď
                if (user.ToString("HH:mm") == DateTime.Now.ToString("HH:mm"))
                {

                        LoadCallLog();
                    Thread.Sleep(60000);
                }

                return true;
            });

        }

    }
}
