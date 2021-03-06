﻿using NHibernate;
using NHibernate.Cfg;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace PeiuPlatform.App
{
    public abstract class EventDataAccessorBase
    {
        public const string ENV_MYSQL_USERNAME = "MYSQL_USERNAME";
        public const string ENV_MYSQL_PASSWORD = "MYSQL_PASSWORD";
        public const string ENV_MYSQL_HOST = "MYSQL_HOST";
        public const string ENV_MYSQL_PORT = "MYSQL_PORT";
        public const string ENV_MYSQL_DATABASE = "MYSQL_DATABASE";
        private ISessionFactory sessionFactory;
        public ISessionFactory SessionFactory => sessionFactory ?? (sessionFactory = GetSessionFactory());

        public static string CreateConnectionString(string host, string port, string database, string username, string password)
            => $"server={host};port={port};userid={username};password={password};database={database};CharSet=utf8;";

        protected abstract  ISessionFactory GetSessionFactory();

        public string ConnectionSting { get; }

        public EventDataAccessorBase(string connectionString)
        {
            this.ConnectionSting = connectionString;
        }

       
    }

    

    public class EventDataAccessor : EventDataAccessorBase
    {
        public static EventDataAccessor CreateDataAccessFromEnvironment()
        {
            string mysqlHost = System.Environment.GetEnvironmentVariable(ENV_MYSQL_HOST);
            string mysqlPort = System.Environment.GetEnvironmentVariable(ENV_MYSQL_PORT);
            string mysqluser = System.Environment.GetEnvironmentVariable(ENV_MYSQL_USERNAME);
            string mysqlPass = System.Environment.GetEnvironmentVariable(ENV_MYSQL_PASSWORD);
            string mysqlDatabase = System.Environment.GetEnvironmentVariable(ENV_MYSQL_DATABASE);
            string mysql_conn = EventDataAccessorBase.CreateConnectionString(mysqlHost, mysqlPort, mysqlDatabase, mysqluser, mysqlPass);
            EventDataAccessor dataAccessor = new EventDataAccessor(mysql_conn);
            return dataAccessor;
        }

        public EventDataAccessor(string connectionString) : base(connectionString) { }
        protected override ISessionFactory GetSessionFactory()
        {
            var config = new Configuration()
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
                            //{NHibernate.Cfg.Environment.ShowSql, "true" }
#endif

                        })
                    .AddAssembly(Assembly.GetExecutingAssembly());

            //foreach (Assembly assembly in assemblies)
            //    config.AddAssembly(assembly);
            return config.BuildSessionFactory();
        }
    }
}
