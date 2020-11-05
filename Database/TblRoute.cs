using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using NLog;

namespace NetCore_Gateway.Database
{
    public class TblRoute
    {
        public long Seq { get; set; }
        public string EndPoint { get; set; }
        public string Path { get; set; }
        public bool RequireAuth { get; set; }

        private readonly static Logger _log = LogManager.GetCurrentClassLogger();
        private static List<TblRoute> _listAll = new List<TblRoute>();
        private static Dictionary<string, TblRoute> _dicRoute = new Dictionary<string, TblRoute>();

        public static async Task Initialize()
        {
            using var conn = new MySqlConnection(Config.Instance.ConnectionString);
            var list = await conn.QueryAsync<TblRoute>("select * from tblRoute");

            var listAll = new List<TblRoute>();
            var dicRoute = new Dictionary<string, TblRoute>();

            foreach (var item in list)
            {
                if (!dicRoute.ContainsKey(item.EndPoint))
                {
                    dicRoute.Add(item.EndPoint, item);
                }

                listAll.Add(item);
            }

            _listAll = listAll;
            _dicRoute = dicRoute;
        }

        public static TblRoute[] ToArray()
        {
            return _listAll.ToArray();
        }

        public static TblRoute Find(string endPoint)
        {
            try
            {
                return _dicRoute[endPoint];
            }
            catch(KeyNotFoundException ex)
            {
                _log.Error(ex.StackTrace);
                return null;
            }
        }

        public static string FindPath(string endPoint)
        {
            try
            {
                return _dicRoute[endPoint].Path;
            }
            catch (KeyNotFoundException ex)
            {
                _log.Error(ex.StackTrace);
                return null;
            }
        }
    }
}
