namespace NetCore_Gateway
{
    public class Config: Singleton<Config>
    {
        public string ConnectionString { get; set; }

        public static void Load(string path)
        {
            Instance = JsonAppConfig.Load<Config>(path);
        }
    }
}
