using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace SqlSample.db
{
  /// <summary>
  /// Data Access Object that uses different ways of accessing sql server
  /// </summary>
  /// <remarks>
  /// EF and pure SQL
  /// </remarks>
  public class DAO : IDisposable
  {
    private readonly MyDBContext dbContext;
    private readonly string userSql;
    private readonly string emailParamName = "@email";

    public DAO()
    {
      this.dbContext = new MyDBContext();
      this.userSql = @"
        SELECT u.id, u.email, u.name, c.total
          FROM users u
        LEFT OUTER JOIN carts c on u.id = c.user_id
         WHERE u.email = " + emailParamName;
    }

    /// <summary>
    /// Get user by email using Entity Framework
    /// </summary>
    /// <remarks>
    /// This call WILL cache results in the database context by default
    /// </remarks>
    /// <param name="email"></param>
    /// <returns></returns>
    public async Task<User> getUserByEmailEF(string email)
    {
      User user = null;

      user = await this.dbContext.users
        .FirstOrDefaultAsync(u => u.email.Equals(email, StringComparison.OrdinalIgnoreCase));

      return user;
    }


    /// <summary>
    /// Get user by email using Entity Framework raw database sql
    /// </summary>
    /// <remarks>
    /// This call WILL NOT cache results in the database context by default
    /// </remarks>
    /// <param name="email"></param>
    /// <returns></returns>
    public async Task<User> getUserByEmailEFRawDbSql(string email)
    {
      User user = null;
      SqlParameter emailParam = new SqlParameter(this.emailParamName, SqlDbType.VarChar, 50);
      emailParam.Value = email;
 
      user = await this.dbContext.Database.SqlQuery<User>(this.userSql, emailParam).FirstOrDefaultAsync();

      return user;
    }


    /// <summary>
    /// Get user by email using Entity Framework raw entity sql
    /// </summary>
    /// <remarks>
    /// This call WILL cache results in the database context by default
    /// </remarks>
    /// <param name="email"></param>
    /// <returns></returns>
    public async Task<User> getUserByEmailEFRawEntitySql(string email)
    {
      User user = null;
      SqlParameter emailParam = new SqlParameter(this.emailParamName, SqlDbType.VarChar, 50);
      emailParam.Value = email;

      user = await this.dbContext.users.SqlQuery(this.userSql, emailParam).FirstOrDefaultAsync();

      return user;
    }


    /// <summary>
    /// Get user by email using pure sql
    /// </summary>
    /// <remarks>
    /// This call will create sql connection
    /// </remarks>
    /// <param name="email"></param>
    /// <returns></returns>
    public async Task<User> getUserByEmailPureSql(string email)
    {
      User user = null;
      // create connection using connection string
      using (SqlConnection connection = new SqlConnection(this.dbContext.Database.Connection.ConnectionString))
      {
        // create sql command from connection
        using (SqlCommand command = connection.CreateCommand())
        {
          // set sql command text and attach parameters
          command.CommandText = this.userSql;
          command.Parameters.Add(new SqlParameter(this.emailParamName, SqlDbType.VarChar, 50)).Value = email;

          await connection.OpenAsync();
          // execute reader and populate response by iterating through each row
          using (SqlDataReader reader = await command.ExecuteReaderAsync())
          {
            if (reader.HasRows)
            {
              user = new User();
              while (reader.Read())
              {
                if (string.IsNullOrWhiteSpace(user.email))
                {
                  user.email = reader["email"] as string;
                }
                user.carts.Add(new Cart { total = reader["total"] as double? });
              }
            }
          }
        }
      }

      return user;
    }


    public void Dispose()
    {
      this.dbContext.Dispose();
    }
  }
}
