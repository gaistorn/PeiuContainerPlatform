using NHibernate;
using NHibernate.Cfg;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Hubbub
{
    public abstract class DataAccessorBase
    {
        public const string ENV_MYSQL_USERNAME = "MYSQL_USERNAME";
        public const string ENV_MYSQL_PASSWORD = "MYSQL_PASSWORD";
        public const string ENV_MYSQL_HOST = "MYSQL_HOST";
        public const string ENV_MYSQL_PORT = "MYSQL_PORT";
        //public const string ENV_MYSQL_DATABASE = "MYSQL_DATABASE";
        private ISessionFactory sessionFactory;
        public ISessionFactory SessionFactory => sessionFactory ?? (sessionFactory = GetSessionFactory());

        public static string CreateConnectionString(string host, string port, string username, string password)
            => $"server={host};port={port};userid={username};password={password};CharSet=utf8;";

        protected abstract ISessionFactory GetSessionFactory();

        public string ConnectionSting { get; }

        public DataAccessorBase(string connectionString)
        {
            this.ConnectionSting = connectionString;
        }


    }



    public class MysqlDataAccessor : DataAccessorBase
    {
        public static MysqlDataAccessor CreateDataAccessFromEnvironment()
        {
            string mysqlHost = System.Environment.GetEnvironmentVariable(ENV_MYSQL_HOST);
            string mysqlPort = System.Environment.GetEnvironmentVariable(ENV_MYSQL_PORT);
            string mysqluser = System.Environment.GetEnvironmentVariable(ENV_MYSQL_USERNAME);
            string mysqlPass = System.Environment.GetEnvironmentVariable(ENV_MYSQL_PASSWORD);
            //string mysqlDatabase = System.Environment.GetEnvironmentVariable(ENV_MYSQL_DATABASE);
            string mysql_conn = DataAccessorBase.CreateConnectionString(mysqlHost, mysqlPort, mysqluser, mysqlPass);
            MysqlDataAccessor dataAccessor = new MysqlDataAccessor(mysql_conn);
            return dataAccessor;
        }

        public MysqlDataAccessor(string connectionString) : base(connectionString) { }
        protected override ISessionFactory GetSessionFactory()
        {
            var config = new NHibernate.Cfg.Configuration()
                        .AddProperties(new Dictionary<string, string> {
                    {NHibernate.Cfg.Environment.ConnectionDriver, typeof (NHibernate.Driver.MySqlDataDriver).FullName},
                   // {NHibernate.Cfg.Environment.ProxyFactoryFactoryClass, typeof (NHibernate.ByteCode.Castle.ProxyFactoryFactory).AssemblyQualifiedName},
                    {NHibernate.Cfg.Environment.Dialect, typeof (NHibernate.Dialect.MySQLDialect).FullName},
                    {NHibernate.Cfg.Environment.ConnectionProvider, typeof (NHibernate.Connection.DriverConnectionProvider).FullName},
                    {NHibernate.Cfg.Environment.ConnectionString, ConnectionSting},
                    //{NHibernate.Cfg.Environment., connectionString},
                            {"hibernate.connection.CharSet", "utf-8"},
                            {"hibernate.connection.characterEncoding", "utf-8" },
                            {"hibernate.connection.useUnicode", "true" },

#if DEBUG
                            {NHibernate.Cfg.Environment.ShowSql, "true" }
#endif

                        })
                    .AddAssembly(Assembly.GetExecutingAssembly());

            //foreach (Assembly assembly in assemblies)
            //    config.AddAssembly(assembly);
            return config.BuildSessionFactory();
        }
    }
}
