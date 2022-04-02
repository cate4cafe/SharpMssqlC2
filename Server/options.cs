using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    internal class options
    {
        internal static string connStr = "Data Source={0};Initial Catalog={1};uid={2};pwd={3};MultipleActiveResultSets=true";
        internal static string host = "172.16.233.175";
        internal static string username = "sa";
        internal static string password = "";
        internal static string dbname = "MssqlC2";
        internal static string table_server = "table_server";
        internal static string table_client = "table_client";
        internal static string c2ip = "xx.xx.xx.xx";
        internal static string c2port = "60050";
        internal static string pipe_name = "mssqlc2";
        internal static byte[] data = Encoding.ASCII.GetBytes("");
    }
}
