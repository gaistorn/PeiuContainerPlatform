﻿//------------------------------------------------------------------------------
// This is auto-generated code.
//------------------------------------------------------------------------------
// This code was generated by Entity Developer tool using NHibernate template.
// Code is generated on: 2020-04-09 오후 1:39:20
//
// Changes to this file may cause incorrect behavior and will be lost if
// the code is regenerated.
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace PeiuPlatform.Models.Mysql
{

    /// <summary>
    /// There are no comments for PeiuPlatform.Models.Mysql.DailyActualRevenue, DatabaseModel in the schema.
    /// </summary>
    public partial class DailyActualRevenue {
    
        #region Extensibility Method Definitions
        
        /// <summary>
        /// There are no comments for OnCreated in the schema.
        /// </summary>
        partial void OnCreated();

        public override bool Equals(object obj)
        {
          DailyActualRevenue toCompare = obj as DailyActualRevenue;
          if (toCompare == null)
          {
            return false;
          }

          if (!Object.Equals(this.Siteid, toCompare.Siteid))
            return false;
          if (!Object.Equals(this.Createdt, toCompare.Createdt))
            return false;
          if (!Object.Equals(this.Rcc, toCompare.Rcc))
            return false;
          
          return true;
        }

        public override int GetHashCode()
        {
          int hashCode = 13;
          hashCode = (hashCode * 7) + Siteid.GetHashCode();
          hashCode = (hashCode * 7) + Createdt.GetHashCode();
          hashCode = (hashCode * 7) + Rcc.GetHashCode();
          return hashCode;
        }
        
        #endregion
        /// <summary>
        /// There are no comments for DailyActualRevenue constructor in the schema.
        /// </summary>
        public DailyActualRevenue()
        {
            OnCreated();
        }

    
        /// <summary>
        /// There are no comments for Siteid in the schema.
        /// </summary>
        public virtual int Siteid
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Createdt in the schema.
        /// </summary>
        public virtual System.DateTime Createdt
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Rcc in the schema.
        /// </summary>
        public virtual int Rcc
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Revenue in the schema.
        /// </summary>
        public virtual double Revenue
        {
            get;
            set;
        }
    }

}
