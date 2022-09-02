using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;

namespace CosmicCoreApi.AccessContext
{
    public class CosmicDbBaseCore : DbContext
    {
        public CosmicDbBaseCore() : base("name=Cosmic_Connection")
        {

        }
        public string GetDbConnectionString
        {
            get
            {
                return Database.Connection.ConnectionString;
            }
        }
    }

    public abstract class CosmicDbBase 
    {
        List<CosmicDbBaseCore> dbConnection = new List<CosmicDbBaseCore>();
        public string ConnectionString
        {
            get
            {
                using (var context = new CosmicDbBaseCore())
                {
                    return context.GetDbConnectionString;
                }
            }
        }
        object objExecuteSqlCommand = new object();
        public int ExecuteSqlCommand(string sql, params object[] parameters)
        {
            lock(objExecuteSqlCommand)
            {
                DisposeoldConnection();
                var conn = new CosmicDbBaseCore();
                var result = conn.Database.ExecuteSqlCommand(sql, parameters);
                dbConnection.Add(conn);
                return result;
            }
        }

        object objSqlQuery = new object();
        public DbRawSqlQuery<TElement> SqlQuery<TElement>(string sql, params object[] parameters)
        {
            lock(objSqlQuery)
            {
                DisposeoldConnection();
                var conn = new CosmicDbBaseCore();
                var result = conn.Database.SqlQuery<TElement>(sql, parameters);
                dbConnection.Add(conn);
                return result;
            }
        }
        object objDisposeoldConnection = new object();
        private void DisposeoldConnection()
        {
            lock (objDisposeoldConnection)
            {
                if(dbConnection.Count>50)
                {
                    dbConnection = new List<CosmicDbBaseCore>();
                }
            }
                
        }
    }
}