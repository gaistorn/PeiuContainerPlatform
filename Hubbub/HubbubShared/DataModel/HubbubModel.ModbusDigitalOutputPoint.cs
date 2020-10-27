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
    /// There are no comments for Hubbub.ModbusDigitalOutputPoint, HubhubSharedLib in the schema.
    /// </summary>
    public partial class ModbusDigitalOutputPoint {
    
        #region Extensibility Method Definitions
        
        /// <summary>
        /// There are no comments for OnCreated in the schema.
        /// </summary>
        partial void OnCreated();

        public override bool Equals(object obj)
        {
          ModbusDigitalOutputPoint toCompare = obj as ModbusDigitalOutputPoint;
          if (toCompare == null)
          {
            return false;
          }

          if (!Object.Equals(this.Hubbubid, toCompare.Hubbubid))
            return false;
          if (!Object.Equals(this.Deviceindex, toCompare.Deviceindex))
            return false;
          if (!Object.Equals(this.Functioncode, toCompare.Functioncode))
            return false;
          if (!Object.Equals(this.Offset, toCompare.Offset))
            return false;
          if (!Object.Equals(this.Commandcode, toCompare.Commandcode))
            return false;
          
          return true;
        }

        public override int GetHashCode()
        {
          int hashCode = 13;
          hashCode = (hashCode * 7) + Hubbubid.GetHashCode();
          hashCode = (hashCode * 7) + Deviceindex.GetHashCode();
          hashCode = (hashCode * 7) + Functioncode.GetHashCode();
          hashCode = (hashCode * 7) + Offset.GetHashCode();
          hashCode = (hashCode * 7) + Commandcode.GetHashCode();
          return hashCode;
        }
        
        #endregion
        /// <summary>
        /// There are no comments for ModbusDigitalOutputPoint constructor in the schema.
        /// </summary>
        public ModbusDigitalOutputPoint()
        {
            this.Functioncode = 3;
            this.Commandorder = 0;
            this.Outputvalue = -1;
            this.Scalefactor = 1f;
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
        /// 3  : Holding Register
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
        /// There are no comments for Commandcode in the schema.
        /// </summary>
        public virtual int Commandcode
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Commandorder in the schema.
        /// </summary>
        public virtual short Commandorder
        {
            get;
            set;
        }

    
        /// <summary>
        /// value * scale
        /// </summary>
        public virtual short? Outputvalue
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Datatypeid in the schema.
        /// </summary>
        public virtual int Datatypeid
        {
            get;
            set;
        }

    
        /// <summary>
        /// 1 : PCS\n2 : BMS\n3 : PV
        /// </summary>
        public virtual int Devicetypeid
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Disable in the schema.
        /// </summary>
        public virtual bool Disable
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
        /// value / scalefactor
        /// </summary>
        public virtual float Scalefactor
        {
            get;
            set;
        }
    }

}
