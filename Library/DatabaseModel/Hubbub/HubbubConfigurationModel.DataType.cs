﻿//------------------------------------------------------------------------------
// This is auto-generated code.
//------------------------------------------------------------------------------
// This code was generated by Entity Developer tool using NHibernate template.
// Code is generated on: 2020-04-09 오후 6:44:00
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

namespace PeiuPlatform.DataAccessor
{

    /// <summary>
    /// There are no comments for PeiuPlatform.DataAccessor.DataType, DatabaseModel in the schema.
    /// </summary>
    public partial class DataType {
    
        #region Extensibility Method Definitions
        
        /// <summary>
        /// There are no comments for OnCreated in the schema.
        /// </summary>
        partial void OnCreated();
        
        #endregion
        /// <summary>
        /// There are no comments for DataType constructor in the schema.
        /// </summary>
        public DataType()
        {
            OnCreated();
        }

    
        /// <summary>
        /// There are no comments for Typeid in the schema.
        /// </summary>
        public virtual int Typeid
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Typename in the schema.
        /// </summary>
        public virtual string Typename
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Sizebybyte in the schema.
        /// </summary>
        public virtual short Sizebybyte
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Isunsigned in the schema.
        /// </summary>
        public virtual bool Isunsigned
        {
            get;
            set;
        }
    }

}
