﻿//------------------------------------------------------------------------------
// This is auto-generated code.
//------------------------------------------------------------------------------
// This code was generated by Entity Developer tool using NHibernate template.
// Code is generated on: 2020-10-26 오후 4:53:53
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
    /// VIEW
    /// </summary>
    public partial class VwDigitalStatusPoint {
    
        #region Extensibility Method Definitions
        
        /// <summary>
        /// There are no comments for OnCreated in the schema.
        /// </summary>
        partial void OnCreated();

        public override bool Equals(object obj)
        {
          VwDigitalStatusPoint toCompare = obj as VwDigitalStatusPoint;
          if (toCompare == null)
          {
            return false;
          }

          if (!Object.Equals(this.Hubbubid, toCompare.Hubbubid))
            return false;
          if (!Object.Equals(this.Deviceindex, toCompare.Deviceindex))
            return false;
          if (!Object.Equals(this.Pcsstatusid, toCompare.Pcsstatusid))
            return false;
          
          return true;
        }

        public override int GetHashCode()
        {
          int hashCode = 13;
          hashCode = (hashCode * 7) + Hubbubid.GetHashCode();
          hashCode = (hashCode * 7) + Deviceindex.GetHashCode();
          hashCode = (hashCode * 7) + Pcsstatusid.GetHashCode();
          return hashCode;
        }
        
        #endregion
        /// <summary>
        /// There are no comments for VwDigitalStatusPoint constructor in the schema.
        /// </summary>
        public VwDigitalStatusPoint()
        {
            OnCreated();
        }

    
        /// <summary>
        /// There are no comments for Hubbubid in the schema.
        /// </summary>
        public virtual int Hubbubid
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Deviceindex in the schema.
        /// </summary>
        public virtual int Deviceindex
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Pcsstatusid in the schema.
        /// </summary>
        public virtual int Pcsstatusid
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Functioncode in the schema.
        /// </summary>
        public virtual int Functioncode
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Offset in the schema.
        /// </summary>
        public virtual int Offset
        {
            get;
            set;
        }

    
        /// <summary>
        /// ?? ?? ??? TRUE ? ???? FALSE? ????
        /// </summary>
        public virtual sbyte Match
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Bitflag in the schema.
        /// </summary>
        public virtual short Bitflag
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Pcsstatusname in the schema.
        /// </summary>
        public virtual string Pcsstatusname
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Pcsstatusdesc in the schema.
        /// </summary>
        public virtual string Pcsstatusdesc
        {
            get;
            set;
        }
    }

}
