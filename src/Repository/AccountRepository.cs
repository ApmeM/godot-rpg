using Godot;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace IsometricGame.Repository
{
    public class AccountRepository
    {
        private const string DefaultLoginFileName = "user://default_login";
        private const string LoginsFileName = "user://logins";

        private static readonly Dictionary<int, string> ActiveLogins = new Dictionary<int, string>();

        public void SaveLogin(string login)
        {
            var file = new File();
            if (file.Open(DefaultLoginFileName, File.ModeFlags.Write) == Error.Ok)
            {
                file.StorePascalString(login);
            }
            file.Close();
        }

        public string LoadLogin()
        {
            var file = new File();
            if (file.Open(DefaultLoginFileName, File.ModeFlags.Read) == Error.Ok)
            {
                var login = file.GetPascalString();
                file.Close();
                return login;
            }

            return null;
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
