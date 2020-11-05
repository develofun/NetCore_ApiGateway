using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NetCore_Gateway.Managers
{
    public class AuthorizationManager
    {
        private static Dictionary<string, Dictionary<string, List<string>>> _dicAuth = new Dictionary<string, Dictionary<string, List<string>>>();

        static AuthorizationManager()
        {
            _dicAuth = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<string>>>>(JsonAppConfig.Load<string>("authorization.json"));
        }

        public static bool CheckEndPointAuth(string endpoint, string method, string permission)
        {
            endpoint = endpoint.ToLower();
            if (!_dicAuth.ContainsKey(endpoint)) return true;


            var methodDict = _dicAuth[endpoint];
            method = method.ToUpper();
            if (!methodDict.ContainsKey(method)) return true;

            return methodDict[method].Contains(permission);
        }

        public static bool CheckEndPointAuth(string endpoint, string method, string[] permissionsArray)
        {
            foreach (string permission in permissionsArray)
            {
                var check = CheckEndPointAuth(endpoint, method, permission);
                if (check) return check;
            }

            return false;
        }
    }
}
