﻿//------------------------------------------------------------------------------
// This is auto-generated code.
//------------------------------------------------------------------------------
// This code was generated by Entity Developer tool using NHibernate template.
// Code is generated on: 2020-03-04 오후 3:13:25
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
    /// There are no comments for PEIU.Models.DatabaseModel.AttachFileRepositary, DatabaseModel in the schema.
    /// </summary>
    public partial class AttachFileRepositary {
    
        #region Extensibility Method Definitions
        
        /// <summary>
        /// There are no comments for OnCreated in the schema.
        /// </summary>
        partial void OnCreated();
        
        #endregion
        /// <summary>
        /// There are no comments for AttachFileRepositary constructor in the schema.
        /// </summary>
        public AttachFileRepositary()
        {
            this.Grouptype = 0;
            this.Downloadcount = 0;
            OnCreated();
        }

    
        /// <summary>
        /// There are no comments for Id in the schema.
        /// </summary>
        public virtual string Id
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Filename in the schema.
        /// </summary>
        public virtual string Filename
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Contents in the schema.
        /// </summary>
        public virtual byte[] Contents
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
        /// There are no comments for Contentstype in the schema.
        /// </summary>
        public virtual string Contentstype
        {
            get;
            set;
        }

    
        /// <summary>
        /// 0 : ????
        /// 1 : ???
        /// </summary>
        public virtual int Grouptype
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Boardid in the schema.
        /// </summary>
        public virtual int Boardid
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Downloadcount in the schema.
        /// </summary>
        public virtual int Downloadcount
        {
            get;
            set;
        }
    }

}
