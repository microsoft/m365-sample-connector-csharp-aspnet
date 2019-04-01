// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConnectorSourceFactoryTest
    {
        [TestMethod]
        public void ConnectorSourceTest()
        {
            ConnectorSourceFactory sourceFactory = new ConnectorSourceFactory();
            IConnectorSourceProvider sourceProvider = sourceFactory.GetConnectorSourceInstance(ConnectorJobType.Facebook);
            Assert.AreEqual(sourceProvider.GetType(), typeof(FacebookProvider));
        }
    }
}
