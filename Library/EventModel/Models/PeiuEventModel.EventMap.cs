﻿//------------------------------------------------------------------------------
// This is auto-generated code.
//------------------------------------------------------------------------------
// This code was generated by Entity Developer tool using NHibernate template.
// Code is generated on: 2020-01-14 오후 3:25:46
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

namespace PeiuPlatform.App
{

    /// <summary>
    /// There are no comments for PeiuPlatform.App.EventMap, EventModel in the schema.
    /// </summary>
    public partial class EventMap {
    
        #region Extensibility Method Definitions
        
        /// <summary>
        /// There are no comments for OnCreated in the schema.
        /// </summary>
        partial void OnCreated();
        
        #endregion
        /// <summary>
        /// There are no comments for EventMap constructor in the schema.
        /// </summary>
        public EventMap()
        {
            this.Level = 4;
            this.Devicetype = 1;
            OnCreated();
        }

    
        /// <summary>
        /// There are no comments for Eventcode in the schema.
        /// </summary>
        public virtual int Eventcode
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Factorycode in the schema.
        /// </summary>
        public virtual int Factorycode
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Bitflag in the schema.
        /// </summary>
        public virtual int Bitflag
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Name in the schema.
        /// </summary>
        public virtual string Name
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Groupcode in the schema.
        /// </summary>
        public virtual int Groupcode
        {
            get;
            set;
        }

    
        /// <summary>
        /// ??? ??
        /// 0 : ??
        /// 1 ~ 3 : ??
        /// 4 ~ : ??
        /// </summary>
        public virtual sbyte Level
        {
            get;
            set;
        }

    
        /// <summary>
        /// 1 : PCS
        /// 2 : BMS
        /// 3 : PV
        /// </summary>
        public virtual int? Devicetype
        {
            get;
            set;
        }
    }

}
