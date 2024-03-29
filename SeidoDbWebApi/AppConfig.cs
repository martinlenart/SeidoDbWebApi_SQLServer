﻿using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbAppWebApi
{
    public sealed class AppConfig
    {
        private static AppConfig _instance = null;
        private static readonly object instanceLock = new();
        private static IConfigurationRoot _configuration;

        private static Dictionary<string,string> _apiKeys;

#if DEBUG
        private string _appsettingfile = "appsettings.Development.json";
#else
        private string _appsettingfile = "appsettings.json";
#endif
        private AppConfig()
        {
            var builder = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile(_appsettingfile, optional: true, reloadOnChange: true)
                                .AddUserSecrets("3d2b8454-7957-4457-9167-d64aaaedb8d3"); //Shared on one developer machine

            _configuration = builder.Build();
        }

        public static IConfigurationRoot ConfigurationRoot
        {
            get
            {
                lock (instanceLock)
                {
                    if (_instance == null)
                    {
                        _instance = new AppConfig();
                        _apiKeys = _configuration.GetSection("apiKeys").GetChildren()
                            .ToDictionary(item=>item.Key, item=>item.Value);
                    }
                    return _configuration;
                }
            }
        }

        public static string CurrentDbType => ConfigurationRoot.GetValue<string>("CurrentDbType");
        public static string CurrentDbConnection => ConfigurationRoot.GetValue<string>("CurrentDbConnection");
        public static string CurrentDbConnectionString => ConfigurationRoot.GetConnectionString(CurrentDbConnection);

        public static bool UseApiKey => ConfigurationRoot.GetValue<string>("UseApiKey") != "Disabled";

        public static bool apiKeyTryGet(string apiKey, out string info)
        {
            info = null;
            if (string.IsNullOrWhiteSpace(apiKey))
                return false;

            return _apiKeys.TryGetValue(apiKey, out info);
        }
    }
}
