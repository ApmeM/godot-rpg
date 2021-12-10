using Godot;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace IsometricGame.Repository
{
    public class AccountRepository
    {
        private const string DefaultLoginFileName = "user://default_login";
        private const string DefaultPasswordFileName = "user://default_password";
        private const string DefaultServerFileName = "user://default_server";
        private const string DefaultIsServerFileName = "user://default_is_server";
        private const string LoginsFileName = "user://logins";

        private static readonly Dictionary<int, string> ActiveLogins = new Dictionary<int, string>();

        public void SaveLogin(string login)
        {
            this.SaveString(DefaultLoginFileName, login);
        }
        
        public void SavePassword(string password)
        {
            this.SaveString(DefaultPasswordFileName, password);
        }

        public void SaveServer(string server)
        {
            this.SaveString(DefaultServerFileName, server);
        }

        public void SaveIsServer(bool isServer)
        {
            this.SaveString(DefaultIsServerFileName, isServer.ToString());
        }

        public string LoadLogin()
        {
            return LoadString(DefaultLoginFileName);
        }
        public string LoadPassword()
        {
            return LoadString(DefaultPasswordFileName);
        }
        public string LoadServer()
        {
            return LoadString(DefaultServerFileName) ?? "91.146.57.100";
        }

        public bool LoadIsServer()
        {
            return bool.Parse(LoadString(DefaultIsServerFileName) ?? "false");
        }

        private string LoadString(string fileName)
        {
            var file = new File();
            if (file.Open(fileName, File.ModeFlags.Read) == Error.Ok)
            {
                var value = file.GetPascalString();
                file.Close();
                return value;
            }

            return null;
        }

        private void SaveString(string fileName, string value)
        {
            var file = new File();
            if (file.Open(fileName, File.ModeFlags.Write) == Error.Ok)
            {
                file.StorePascalString(value);
            }
            file.Close();
        }

        public Dictionary<string, string> LoadCredentials()
        {
            var file = new File();
            if (file.Open(LoginsFileName, File.ModeFlags.Read) == Error.Ok)
            {
                var creds = JsonConvert.DeserializeObject<Dictionary<string, string>>(file.GetPascalString());
                file.Close();
                return creds;
            }

            return new Dictionary<string, string> { { "Server", "" } };
        }

        public void InitializeActiveLogins()
        {
            ActiveLogins.Clear();
            ActiveLogins[1] = "Server";
        }

        public void SaveCredentials(Dictionary<string, string> credentials)
        {
            var file = new File();
            if (file.Open(LoginsFileName, File.ModeFlags.Write) == Error.Ok)
            {
                var creds = JsonConvert.SerializeObject(credentials);
                file.StorePascalString(creds);
                file.Close();
            }
        }

        public void AddActiveLogin(int clientId, string login)
        {
            ActiveLogins[clientId] = login;
        }

        public void RemoveActiveLogin(int clientId)
        {
            ActiveLogins.Remove(clientId);
        }

        public string FindForClientActiveLogin(int clientId)
        {
            if (!ActiveLogins.ContainsKey(clientId))
            {
                return null;
            }

            return ActiveLogins[clientId];
        }

    }
}
