// ***********************************************************************
// <copyright file="IBackgroundCopyError.cs" project="SharpBits.Base" assembly="SharpBits.Base" solution="SevenUpdate" company="Xidar Solutions">
//     Copyright (c) xidar solutions. All rights reserved.
// </copyright>
// <author username="xidar">xidar</author>
// <author username="sevenalive">Robert Baker</author>
// <license href="http://sharpbits.codeplex.com/license">BSD License</license> 
// ***********************************************************************

namespace SharpBits.Base
{
    using System.Runtime.InteropServices;

    /// <summary>Use the information in the IBackgroundCopyError interface to determine the cause of the error and if the transferprocess can proceed.</summary>
    [Guid("19C613A0-FCB8-4F28-81AE-897C3D078F81"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown), ComImportAttribute]
    internal interface IBackgroundCopyError
    {
        /// <summary>Retrieves the error code and identify the contextin which the error occurred.</summary>
        /// <param name="context">Context in which the error occurred. For a list of context values, see the <see cref="BGErrorContext" /> enumeration.</param>
        /// <param name="code">Error code of the error that occurred.</param>
        void GetError(out BGErrorContext context, [MarshalAs(UnmanagedType.Error)] out int code);

        /// <summary>Retrieves an interface pointer to the file objectassociated with the error.</summary>
        /// <param name="val">An <see cref="IBackgroundCopyFile" /> interface pointer whose methods you use to determine the local and remote file names associated with the error. The file parameter is set to <see langword="null" /> if the error is not associated with the local or remote file. When done, release file.</param>
        void GetFile([MarshalAs(UnmanagedType.Interface)] out IBackgroundCopyFile val);

        /// <summary>Retrieves the error text associated with the error.</summary>
        /// <param name="languageId">Identifies the locale to use to generate the description. To create the language identifier, use the MAKELANGID macro.</param>
        /// <param name="errorDescription">A string that contains the error text associated with the error. Call the CoTaskMemFree function to free <paramref name="errorDescription" /> when done.</param>
        void GetErrorDescription(uint languageId, [MarshalAs(UnmanagedType.LPWStr)] out string errorDescription);

        /// <summary>Retrieves a description of the context in which the error occurred.</summary>
        /// <param name="languageId">Identifies the locale to use to generate the description. To create the language identifier, use the MAKELANGID macro.</param>
        /// <param name="contextDescription">A string that contains the description of the context in which the error occurred. Call the CoTaskMemFree function to free ppContextDescription when done.</param>
        void GetErrorContextDescription(uint languageId, [MarshalAs(UnmanagedType.LPWStr)] out string contextDescription);

        /// <summary>Retrieves the protocol used to transfer the file.</summary>
        /// <param name="protocol">A string that contains the protocol used to transfer the file. The string contains HTTP for the HTTP protocol and file for the SMB protocol. The ppProtocol parameter is set to <see langword="null" /> if the error is not related to the transfer protocol. Call the CoTaskMemFree function to free ppProtocol when done.</param>
        void GetProtocol([MarshalAs(UnmanagedType.LPWStr)] out string protocol);
    }
}