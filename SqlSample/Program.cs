using Newtonsoft.Json;
using SqlSample.db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSample
{
  /// <summary>
  /// Sample program to show different ways .NET can be integrated with SQL server.
  /// Please see db.MyDBContext and db.DAO for implementation details
  /// </summary>
  class Program
  {
    static void Main(string[] args)
    {
      MainAsync(args).Wait();
      Console.WriteLine("Done executing");
      Console.ReadLine();
    }


    /// <summary>
    /// This method executes dao methods asynchronously
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    static async Task MainAsync(string[] args)
    {
      // dictionary to hold dao method delegates
      Dictionary<string, Func<string, Task<User>>> daoMethods = new Dictionary<string, Func<string, Task<User>>>();
      string carolEmail = "carol@noemail.com";

      using (DAO dao = new DAO())
      {
        daoMethods.Add("Get using EF", dao.getUserByEmailEF);
        daoMethods.Add("Get using EF database raw sql", dao.getUserByEmailEFRawDbSql);
        daoMethods.Add("Get using EF entity raw sql", dao.getUserByEmailEFRawEntitySql);
        daoMethods.Add("Get using pure sql", dao.getUserByEmailPureSql);

        foreach (string methodDescription in daoMethods.Keys)
        {
          // get dao method delegate and execute it
          // serialize results and display in console
          try
          {
            var daoMethod = daoMethods[methodDescription];
            User user = await daoMethod.Invoke(carolEmail);
            Console.WriteLine("{0}:\t{1}\n", methodDescription, JsonConvert.SerializeObject(user));
          }
          catch (Exception ex)
          {
            Console.WriteLine("Exception occured: {0}", ex.Message);
          }
        }
      }
    }
  }
}
