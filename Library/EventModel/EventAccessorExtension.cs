using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.App
{
    public static class EventAccessorExtension
    {
       
        public static void AddEventDataAccessor(this IServiceCollection services)
        {
            EventDataAccessor dataAccessor = null;
#if DEBUG
            string mysql_conn = EventDataAccessorBase.CreateConnectionString("192.168.0.40", "3306", "peiugrid", "power21", "123qwe");
            dataAccessor = new EventDataAccessor(mysql_conn);
#else
            dataAccessor = EventDataAccessor.CreateDataAccessFromEnvironment();
#endif
            services.AddSingleton<EventDataAccessor>(dataAccessor);
        }
    }
}
