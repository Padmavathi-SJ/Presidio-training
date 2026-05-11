using Npgsql;
using System.Net.NetworkInformation;

namespace UnderstandingADOApp
{
    
    internal class Program
    {
        string connectionString =
            "Host=localhost;Port=5432;Database=test_db;Username=postgres;Password=Padmavathi5743@";
        NpgsqlConnection connection;
        public Program()
        {
          connection = new NpgsqlConnection(connectionString);
           
        }
        void GetProductDataFromDatabase()
        {
            string selectQuery = "Select * from Products";
            NpgsqlCommand command = new NpgsqlCommand(selectQuery, connection);
            try
            {
                connection.Open();
                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine("Product Id : " + reader[0].ToString());
                    Console.WriteLine("Product Name : " + reader[1].ToString());
                }
                Console.WriteLine("Done reading");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                connection?.Close();
            }

        }

        void GetUserDataFromDatabase()
        {
            string selectQuery = "select * from users";
            NpgsqlCommand command = new NpgsqlCommand(selectQuery, connection);
            try
            {
                connection.Open();
                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine("user id: " + reader[0]);
                    Console.WriteLine("user name: " + reader[1].ToString());
                    Console.WriteLine("user password: "+ reader[2].ToString());
                    Console.WriteLine("user role: "+ reader[3].ToString());
                }
                Console.WriteLine("reading done");
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                connection?.Close();
            }
        }

        void InsertUserInToDatabase()
        {
            
            User user = GetUserDataFromConsole();
            string insertCmd = $"Insert into Users (user_name, user_password, user_role) values ('{user.Username}','{user.Password}','{user.Role}')";
            NpgsqlCommand command = new NpgsqlCommand(insertCmd, connection);
            try
            {
                connection.Open();
                int result = command.ExecuteNonQuery();
                if(result>0)
                    Console.WriteLine("User created successfully");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                connection?.Close();
            }
        }

        private User GetUserDataFromConsole()
        {
            User user = new User();
            Console.WriteLine("Please eneter your preffered username");
            user.Username = Console.ReadLine()??"";
            Console.WriteLine("Please eneter teh password");
            user.Password = Console.ReadLine()??"";
            Console.WriteLine("Please eneter your role");
            user.Role = Console.ReadLine() ?? "";
            return user;

        }


        void UpdateUserPassword()
        {
            GetUserDataFromDatabase();
            User user = GetUserPassword();
            string updateCmd = $"update users set user_password = '{user.Password}' where user_id = {user.UserId}";
              NpgsqlCommand command = new NpgsqlCommand(updateCmd, connection);
            try
            {
                connection.Open();
                int result = command.ExecuteNonQuery();
                if(result>0)
                    Console.WriteLine("password updated successfully");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                connection?.Close();
            }
        }


        private User GetUserPassword()
        {
            User user = new User();
            Console.WriteLine("enter user_id to change password: ");
            user.UserId = int.Parse(Console.ReadLine() ?? "0");
            Console.WriteLine("enter new password: ");
            user.Password = Console.ReadLine().ToString();
            return user;
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            new Program().UpdateUserPassword();

        }
    }
    public class User
    {
        public int UserId {get; set; } = 0;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
