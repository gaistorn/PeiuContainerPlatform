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
    /// There are no comments for PeiuPlatform.DataAccessor.ProtocolType, DatabaseModel in the schema.
    /// </summary>
    public partial class ProtocolType {
    
        #region Extensibility Method Definitions
        
        /// <summary>
        /// There are no comments for OnCreated in the schema.
        /// </summary>
        partial void OnCreated();
        
        #endregion
        /// <summary>
        /// There are no comments for ProtocolType constructor in the schema.
        /// </summary>
        public ProtocolType()
        {
            OnCreated();
        }

    
        /// <summary>
        /// There are no comments for Uniqueid in the schema.
        /// </summary>
        public virtual int Uniqueid
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Protocolname in the schema.
        /// </summary>
        public virtual string Protocolname
        {
            get;
            set;
        }
    }

}
