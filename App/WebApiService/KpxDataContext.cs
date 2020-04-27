using Microsoft.Extensions.Configuration;
using PEIU.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PeiuPlatform.App
{
    public class KpxDataContext
    {
        public NHibernate.ISessionFactory SessionFactory => _da.SessionFactory;
        MysqlDataAccess _da;
        public KpxDataContext(IConfiguration configuration)
        {
            string mysql_conn = configuration.GetConnectionString("kpx");

            _da = new MysqlDataAccess(mysql_conn);
        }
    }
}
