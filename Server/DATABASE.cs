using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Server
{
    public class DATABASE
    {
       public static byte[] Select_data(SqlConnection sqlConnection, string table,int id)
        {
            byte[] data = Encoding.ASCII.GetBytes("");
            string sqlStr = @"select data from {0} where id=@number";
            SqlCommand sqlCommand = new SqlCommand(string.Format(sqlStr, table), sqlConnection);
            
                sqlCommand.Parameters.Add("@number", SqlDbType.Int).Value = id;
                try
                {
                    if (sqlConnection.State != ConnectionState.Open)
                    {
                        sqlConnection.Open();
                    }
                    SqlDataReader dr = sqlCommand.ExecuteReader();
                    if (dr.Read())
                    {
                        data = (byte[])dr["data"];
                    }
                    dr.Close();
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                return data;
            

        }

        public static void insert_data(SqlConnection sqlConnection, string table, byte[] data)
        {
            string sqlStr = @"insert into {0}(data) values(@data)";
            using (SqlCommand sqlCommand = new SqlCommand(string.Format(sqlStr, table), sqlConnection))
            {
                sqlCommand.Parameters.Add("@data",SqlDbType.VarBinary).Value = data;
                try
                {
                    if (sqlConnection.State != ConnectionState.Open)
                    {
                        sqlConnection.Open();
                    }
                    sqlCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public static void CreateDB()
        {
            try
            {
                SqlConnection sqlConnection = new SqlConnection(string.Format(options.connStr, options.host, "master", options.username, options.password));
                string sqlStr = @"CREATE DATABASE {0} ON PRIMARY(NAME = {1}, FILENAME = 'C:\\users\\public\\MssqlC2.mdf', SIZE = 2MB, MAXSIZE = 1000MB, FILEGROWTH = 10%)LOG ON (NAME ={2}_log , FILENAME = 'C:\\users\\public\\MssqlC2.ldf', MAXSIZE = 1000MB, FILEGROWTH = 10%)";
                Console.WriteLine(string.Format(sqlStr, options.dbname, options.dbname, options.dbname));
                SqlCommand sqlCommand = new SqlCommand(string.Format(sqlStr, options.dbname, options.dbname, options.dbname), sqlConnection);
                try
                {
                    sqlConnection.Open();
                    sqlCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                sqlConnection = new SqlConnection(string.Format(options.connStr, options.host, options.dbname, options.username, options.password));
                sqlStr = @"CREATE TABLE {0} (id int PRIMARY KEY IDENTITY(1,1),data varbinary(max));CREATE TABLE {1} (id int PRIMARY KEY IDENTITY(1,1),data varbinary(max))";
                sqlCommand = new SqlCommand(string.Format(sqlStr, options.table_server, options.table_client), sqlConnection);
                try
                {
                    if (sqlConnection.State == ConnectionState.Closed)
                    {
                        sqlConnection.Open();
                    }
                    sqlCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }
}
