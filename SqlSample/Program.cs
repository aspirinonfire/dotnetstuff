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
      string email = "alex@noemail.com";

      using (DAO dao = new DAO())
      {
        daoMethods.Add("Get User using EF", dao.getUserByEmailEF);
        daoMethods.Add("Get User using EF database raw sql", dao.getUserByEmailEFRawDbSql);
        daoMethods.Add("Get User using EF entity raw sql", dao.getUserByEmailEFRawEntitySql);
        daoMethods.Add("Get User using pure sql", dao.getUserByEmailPureSql);

        foreach (string methodDescription in daoMethods.Keys)
        {
          // get dao method delegate and execute it
          // serialize results and display in console
          var daoMethod = daoMethods[methodDescription];
          await waitResult(daoMethod, methodDescription, email);
        }

        // run recursive query
        await waitResult(dao.getCategoryHierarchy, "Recursive query result", null);

        // execute upsert commands
        string visitorname = "alex";
        await waitResult(dao.upsertVisitor, "Upsert visitor (insert)", visitorname);
        await waitResult(dao.upsertVisitor, "Upsert visitor (update)", visitorname);
      }
    }

    /// <summary>
    /// This method invokes a delegate and displays execution results
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="method"></param>
    /// <param name="methodDescription"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    private static async Task waitResult<T>(Func<string, Task<T>> method, string methodDescription, string param)
    {
      try
      {
        T result = await method.Invoke(param);
        Console.WriteLine("===== {0} =====\n{1}\n", methodDescription, JsonConvert.SerializeObject(result, Formatting.Indented));
      }
      catch (Exception ex)
      {
        Console.WriteLine("Exception occurred: {0}", ex.Message);
      }
    }
  }
}
