using Microsoft.Extensions.Configuration;
using PEIU.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PeiuPlatform.App
{
    public class GridDataContext
    {
        public NHibernate.ISessionFactory SessionFactory => _da.SessionFactory;
        public virtual string GetConnectionString(IConfiguration configuration)
        {
            string mysql_conn = configuration.GetConnectionString("peiudb");
            return mysql_conn;
        }
        MysqlDataAccess _da;
        public GridDataContext(IConfiguration configuration)
        {
            string mysql_conn = GetConnectionString(configuration);

            Assembly addAssembly = System.Reflection.Assembly.GetAssembly(typeof(Events.Alarm.EventModel));
            _da = new MysqlDataAccess(mysql_conn, addAssembly);
        }
    }

    public class PeiuGridDataContext
    {
        public NHibernate.ISessionFactory SessionFactory => _da.SessionFactory;
        public virtual string GetConnectionString(IConfiguration configuration)
        {
            string mysql_conn = configuration.GetConnectionString("peiugriddb");
            return mysql_conn;
        }
        MysqlDataAccess _da;
        public PeiuGridDataContext(IConfiguration configuration)
        {
            string mysql_conn = GetConnectionString(configuration);

            Assembly addAssembly = System.Reflection.Assembly.GetAssembly(typeof(Events.Alarm.EventModel));
            _da = new MysqlDataAccess(mysql_conn, addAssembly);
        }
    }
}
