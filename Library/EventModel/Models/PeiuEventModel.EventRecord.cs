﻿//------------------------------------------------------------------------------
// This is auto-generated code.
//------------------------------------------------------------------------------
// This code was generated by Entity Developer tool using NHibernate template.
// Code is generated on: 2020-09-21 오후 5:54:37
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
    /// ??? ??? ?? ??
    /// </summary>
    public partial class EventRecord {
    
        #region Extensibility Method Definitions
        
        /// <summary>
        /// There are no comments for OnCreated in the schema.
        /// </summary>
        partial void OnCreated();
        
        #endregion
        /// <summary>
        /// There are no comments for EventRecord constructor in the schema.
        /// </summary>
        public EventRecord()
        {
            OnCreated();
        }

    
        /// <summary>
        /// ??? ??? ?? ID
        /// </summary>
        public virtual int Eventrecordindex
        {
            get;
            set;
        }

    
        /// <summary>
        /// siteid
        /// </summary>
        public virtual int Siteid
        {
            get;
            set;
        }

    
        /// <summary>
        /// EventMap-eventcode(?????ID)
        /// </summary>
        public virtual int Eventcode
        {
            get;
            set;
        }

    
        /// <summary>
        /// ??? ?? ??
        /// </summary>
        public virtual System.DateTime Createts
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Recoveryts in the schema.
        /// </summary>
        public virtual System.DateTime? Recoveryts
        {
            get;
            set;
        }

    
        /// <summary>
        /// ??? ??(ack)? ??
        /// </summary>
        public virtual System.DateTime? Ackts
        {
            get;
            set;
        }

    
        /// <summary>
        /// ??(ack)? ???
        /// </summary>
        public virtual string Ackuser
        {
            get;
            set;
        }

    
        /// <summary>
        /// 1 : PCS,
        /// 2 : BMS
        /// 3 : PV
        /// </summary>
        public virtual int Devicetype
        {
            get;
            set;
        }

    
        /// <summary>
        /// ?? ??
        /// </summary>
        public virtual int Deviceindex
        {
            get;
            set;
        }

    
        /// <summary>
        /// ??(ack)? ?? ???
        /// </summary>
        public virtual string Ackemail
        {
            get;
            set;
        }
    }

}
