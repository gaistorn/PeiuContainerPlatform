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
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PeiuPlatform.Model.Database
{

    [Table("vw_contractorusers")]
    /// <summary>
    /// VIEW
    /// </summary>

    public partial class VwContractoruserEF : VwContractoruserBase
    {
    
        #region Extensibility Method Definitions
        
        /// <summary>
        /// There are no comments for OnCreated in the schema.
        /// </summary>
        partial void OnCreated();
        
        #endregion
        /// <summary>
        /// There are no comments for VwContractoruser constructor in the schema.
        /// </summary>
        public VwContractoruserEF()
        {
            OnCreated();
        }

    
        [Key]
        /// <summary>
        /// There are no comments for UserId in the schema.
        /// </summary>
        public override string UserId
        {
            get;
            set;
        }
    }

    public class VwContractoruserBase : INotifyPropertyChanged
    {
        #region Extensibility Method Definitions

        #endregion
        /// <summary>
        /// There are no comments for VwContractoruser constructor in the schema.
        /// </summary>
        public VwContractoruserBase()
        {
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
        /// There are no comments for FirstName in the schema.
        /// </summary>
        public virtual string FirstName
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
        /// There are no comments for CompanyName in the schema.
        /// </summary>
        public virtual string CompanyName
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
        /// There are no comments for RegistDate in the schema.
        /// </summary>
        public virtual System.DateTime RegistDate
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
        /// There are no comments for Expire in the schema.
        /// </summary>
        public virtual System.DateTime Expire
        {
            get;
            set;
        }

        private bool _SignInConfirm = false;
        /// <summary>
        /// There are no comments for SignInConfirm in the schema.
        /// </summary>
        public virtual bool SignInConfirm
        {
            get => _SignInConfirm;
            set
            {
                _SignInConfirm = value;
                OnPropertyChanged("SignInConfirm");
            }
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
        /// There are no comments for Email in the schema.
        /// </summary>
        public virtual string Email
        {
            get;
            set;
        }


        private ContractStatusCodes _status;
        /// <summary>
        /// There are no comments for ContractStatus in the schema.
        /// </summary>
        public virtual ContractStatusCodes ContractStatus
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged("ContractStatus");
            }
        }


        /// <summary>
        /// There are no comments for Representation in the schema.
        /// </summary>
        public virtual string Representation
        {
            get;
            set;
        }

        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
