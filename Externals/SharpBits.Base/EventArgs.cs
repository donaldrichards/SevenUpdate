﻿// ***********************************************************************
// <copyright file="EventArgs.cs"
//            project="SharpBits.Base"
//            assembly="SharpBits.Base"
//            solution="SevenUpdate"
//            company="Xidar Solutions">
//     Copyright (c) xidar solutions. All rights reserved.
// </copyright>
// <author username="xidar">xidar</author>
// <author username="sevenalive">Robert Baker</author>
// <license href="http://sharpbits.codeplex.com/license">BSD License</license> 
// ***********************************************************************
namespace SharpBits.Base
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    using SharpBits.Base.Job;

    /// <summary>The event data for the JobNotification event</summary>
    public class JobNotificationEventArgs : EventArgs
    {
    }

    /// <summary>The event data for the ErrorNotification event</summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "EventArgs")]
    public class ErrorNotificationEventArgs : NotificationEventArgs
    {
        #region Constructors and Destructors

        /// <summary>Initializes a new instance of the <see cref="ErrorNotificationEventArgs"/> class.</summary>
        /// <param name="job">The job the notification is for</param>
        /// <param name="error">The error that occurred</param>
        internal ErrorNotificationEventArgs(BitsJob job, BitsError error) : base(job)
        {
            this.Error = error;
        }

        #endregion

        #region Properties

        /// <summary>Gets the error.</summary>
        /// <value>The error that occurred</value>
        public BitsError Error { get; private set; }

        #endregion
    }

    /// <summary>The event data for the interface notification event</summary>
    public class BitsInterfaceNotificationEventArgs : NotificationEventArgs
    {
        #region Constants and Fields

        /// <summary>The Com exception</summary>
        private readonly COMException exception;

        #endregion

        #region Constructors and Destructors

        /// <summary>Initializes a new instance of the <see cref="BitsInterfaceNotificationEventArgs"/> class.</summary>
        /// <param name="job">The job the notification is for</param>
        /// <param name="exception">The exception.</param>
        /// <param name="description">The description.</param>
        internal BitsInterfaceNotificationEventArgs(BitsJob job, COMException exception, string description) : base(job)
        {
            this.Description = description;
            this.exception = exception;
        }

        #endregion

        #region Properties

        /// <summary>Gets the description.</summary>
        /// <value>The description.</value>
        public string Description { get; private set; }

        /// <summary>Gets the message.</summary>
        /// <value>The message.</value>
        public string Message
        {
            get
            {
                return this.exception.Message;
            }
        }

        /// <summary>Gets the error code</summary>
        /// <value>The error code</value>
        public int Result
        {
            get
            {
                return this.exception.ErrorCode;
            }
        }

        #endregion
    }
}