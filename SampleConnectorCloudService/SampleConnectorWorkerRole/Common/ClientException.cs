// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System;

    [Serializable]
    public class ClientException<Error> : Exception
    {
        public Error error { get; private set; }

        public ClientException(Error error)
        {
            this.error = error;
        }
    }
}