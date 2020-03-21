using System.IO;
using Tomlyn;
using Tomlyn.Model;
using Tomlyn.Syntax;
using Unity.Networking.Transport;
using UnityEngine;

public struct ServerConfig
{
    public string Ip;
    public long Port;

    public NetworkEndPoint EndPoint
        => NetworkEndPoint.Parse(Ip, (ushort)Port);

    public static ServerConfig Default => new ServerConfig { Ip = "192.168.1.83", Port = 8787 };
}

public static class ConfigFile
{
    public static readonly string CONFIG_FILE_PATH = Application.dataPath + "/settings.toml";

    public static void Save(ServerConfig config)
    {
        var input = new DocumentSyntax()
        {
            Tables = { new TableSyntax("server") {
                    Items = {
                        { "ip", config.Ip },
                        { "port", config.Port },
                    }
                }
            }
        };
        File.WriteAllText(CONFIG_FILE_PATH, input.ToString());
    }

    public static bool Exists()
    {
        return File.Exists(CONFIG_FILE_PATH);
    }

    public static bool Load(out ServerConfig config)
    {
        config = ServerConfig.Default;

        if (!Exists()) return false;

        var table = Toml.Parse(File.ReadAllText(CONFIG_FILE_PATH)).ToModel();

        config.Ip = (string)((TomlTable)table["server"])["ip"];
        config.Port = (long)((TomlTable)table["server"])["port"];

        return true;
    }
}
