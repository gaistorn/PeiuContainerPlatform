﻿//------------------------------------------------------------------------------
// This is auto-generated code.
//------------------------------------------------------------------------------
// This code was generated by Entity Developer tool using NHibernate template.
// Code is generated on: 2019-09-09 오전 10:39:15
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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeiuPlatform.Model.Database
{
    [Table("vw_aggregatorusers")]
    public class VwAggregatoruserEF : VwAggregatoruserBase
    {
        [Key]
        public override string UserId { get => base.UserId; set => base.UserId = value; }
    }

    public partial class VwAggregatoruserBase {
    
        #region Extensibility Method Definitions
        
        /// <summary>
        /// There are no comments for OnCreated in the schema.
        /// </summary>
        partial void OnCreated();
        
        #endregion
        /// <summary>
        /// There are no comments for VwAggregatoruser constructor in the schema.
        /// </summary>
        public VwAggregatoruserBase()
        {
            OnCreated();
        }

    
        /// <summary>
        /// There are no comments for UserId in the schema.
        /// </summary>
        public virtual string UserId
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for AggGroupId in the schema.
        /// </summary>
        public virtual string AggGroupId
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for AggName in the schema.
        /// </summary>
        public virtual string AggName
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Representation in the schema.
        /// </summary>
        public virtual string Representation
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for UserName in the schema.
        /// </summary>
        public virtual string UserName
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Email in the schema.
        /// </summary>
        public virtual string Email
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for PhoneNumber in the schema.
        /// </summary>
        public virtual string PhoneNumber
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for FirstName in the schema.
        /// </summary>
        public virtual string FirstName
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for CompanyName in the schema.
        /// </summary>
        public virtual string CompanyName
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for LastName in the schema.
        /// </summary>
        public virtual string LastName
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for RegistrationNumber in the schema.
        /// </summary>
        public virtual string RegistrationNumber
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Address in the schema.
        /// </summary>
        public virtual string Address
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for RegistDate in the schema.
        /// </summary>
        public virtual System.DateTime RegistDate
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Expire in the schema.
        /// </summary>
        public virtual System.DateTime Expire
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for SignInConfirm in the schema.
        /// </summary>
        public virtual bool SignInConfirm
        {
            get;
            set;
        }
    }

}
