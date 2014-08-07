using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iContrAll.TcpServer
{

    class DataAccesLayer: IDisposable
    {
        private string connectionString = string.Empty;
        private string databaseName = string.Empty;
        private MySqlConnection mysqlConn;


        // TODO:
        //      legyen "using" minden lekéréskor a DbConnection-re
        //      Comment: most inkább IDisposable-t valósít meg így, bármit el lehet érni, szebb lesz.
        //void Connect()
        //{
        //    try
        //    {
        //        connectionString = System.Configuration.ConfigurationSettings.AppSettings["mysqlConnectionString"].ToString();

        //        mysqlConn = new MySqlConnection(connectionString);
        //        mysqlConn.Open();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message.ToString());
        //    }
        //}

        public DataAccesLayer()
        {
            try
            {
                connectionString = ConfigurationManager.AppSettings["mysqlConnectionString"].ToString();

                mysqlConn = new MySqlConnection(connectionString);
                mysqlConn.Open();
            }
            catch (Exception ex)
            {
                // TODO: normális hibakezelő modult írni!
                Console.WriteLine(ex.Message.ToString());
            }
        }

        #region Queries
        public IEnumerable<Device> GetDeviceList()
        {
            // TODO:
            //      MINDEN RESULT-ot ELLENŐRIZNI!!! (típus, try-catch)


            List<Device> returnList = new List<Device>();

            using (var cmd = mysqlConn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Device";
                
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        returnList.Add(new Device { Id = reader["Id"].ToString(), Channel = int.Parse(reader["Channel"].ToString()), Name = reader["Name"].ToString() });
                    }
                }
            }

            return returnList;
        }

        public void AddDevice(string id, int channel, string name)
        {
            using (MySqlCommand cmd = mysqlConn.CreateCommand())
            {
                var count = GetDeviceList().Count(d => d.Id == id && d.Channel == channel);
                if (count > 0)
                {
                    // UPDATE
                    cmd.CommandText = "UPDATE Device SET Name = @Name WHERE Id = @Id AND Channel = @Channel";
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Channel", channel);
                    cmd.Parameters.AddWithValue("@Name", name);
                }
                else
                {
                    // INSERT
                    cmd.CommandText = "INSERT INTO Device(Id,Channel,Name) VALUES(@Id, @Channel, @Name)";
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Channel", channel);
                    cmd.Parameters.AddWithValue("@Name", name);
                }

                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        #region IDisposable members
        public void Dispose()
        {
            mysqlConn.Close();
        }

        #endregion
    }

}
