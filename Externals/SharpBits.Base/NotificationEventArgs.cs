﻿// ***********************************************************************
// <copyright file="NotificationEventArgs.cs"
//            project="SharpBits.Base"
//            assembly="SharpBits.Base"
//            solution="SevenUpdate"
//            company="Xidar Solutions">
//     Copyright (c) xidar solutions. All rights reserved.
// </copyright>
// <author username="xidar">xidar</author>
// ***********************************************************************
namespace SharpBits.Base
{
    using SharpBits.Base.Job;

    /// <summary>
    /// The event data for the Notification event
    /// </summary>
    public class NotificationEventArgs : JobNotificationEventArgs
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationEventArgs"/> class.
        /// </summary>
        /// <param name="job">
        /// The job the event occurred for
        /// </param>
        internal NotificationEventArgs(BitsJob job)
        {
            this.Job = job;
        }

        #endregion

        #region Properties

        /// <summary>
        ///   Gets the job.
        /// </summary>
        /// <value>The <see cref = "BitsJob" /> the notification occurred for</value>
        public BitsJob Job { get; private set; }

        #endregion
    }
}