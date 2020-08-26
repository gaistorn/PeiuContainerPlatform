using Microsoft.Extensions.Configuration;
using PeiuPlatform.DataAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PeiuPlatform.App
{
    public class MysqlDataContext
    {
        public NHibernate.ISessionFactory SessionFactory => _da.SessionFactory;

        public NHibernate.ISessionFactory GetSessionFactoryWithAssemblies(params Assembly[] assemblies)
        {
            return _da.GetSessionFactoryWithAssemblies(assemblies);
        }

        MysqlDataAccessor _da;
        public MysqlDataContext(IConfiguration configuration)
        {
            //string mysql_conn = configuration.GetConnectionString("peiu_account_connnectionstring");
            Assembly addAssembly = System.Reflection.Assembly.Load("DatabaseModel");
            _da = MysqlDataAccessor.CreateDataAccessFromEnvironment();
        }
    }
}
