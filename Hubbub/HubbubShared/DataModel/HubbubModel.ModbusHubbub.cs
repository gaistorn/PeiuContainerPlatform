﻿//------------------------------------------------------------------------------
// This is auto-generated code.
//------------------------------------------------------------------------------
// This code was generated by Entity Developer tool using NHibernate template.
// Code is generated on: 2020-08-10 오전 10:53:32
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

namespace Hubbub
{

    /// <summary>
    /// There are no comments for Hubbub.ModbusHubbub, HubhubSharedLib in the schema.
    /// </summary>
    public partial class ModbusHubbub {
    
        #region Extensibility Method Definitions
        
        /// <summary>
        /// There are no comments for OnCreated in the schema.
        /// </summary>
        partial void OnCreated();

        public override bool Equals(object obj)
        {
          ModbusHubbub toCompare = obj as ModbusHubbub;
          if (toCompare == null)
          {
            return false;
          }

          if (!Object.Equals(this.Id, toCompare.Id))
            return false;
          if (!Object.Equals(this.Siteid, toCompare.Siteid))
            return false;
          
          return true;
        }

        public override int GetHashCode()
        {
          int hashCode = 13;
          hashCode = (hashCode * 7) + Id.GetHashCode();
          hashCode = (hashCode * 7) + Siteid.GetHashCode();
          return hashCode;
        }
        
        #endregion
        /// <summary>
        /// There are no comments for ModbusHubbub constructor in the schema.
        /// </summary>
        public ModbusHubbub()
        {
            this.Firmwareversion = 1f;
            OnCreated();
        }

    
        /// <summary>
        /// There are no comments for Id in the schema.
        /// </summary>
        public virtual int Id
        {
            get;
            set;
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
        /// There are no comments for Firmwareversion in the schema.
        /// </summary>
        public virtual float Firmwareversion
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Label in the schema.
        /// </summary>
        public virtual string Label
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Description in the schema.
        /// </summary>
        public virtual string Description
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Installationdt in the schema.
        /// </summary>
        public virtual System.DateTime Installationdt
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Maintenancedt in the schema.
        /// </summary>
        public virtual System.DateTime? Maintenancedt
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Connectionid in the schema.
        /// </summary>
        public virtual int Connectionid
        {
            get;
            set;
        }
    }

}
