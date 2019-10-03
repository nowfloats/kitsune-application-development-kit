using Kitsune.API2.DataHandlers.Mongo;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kitsune.API2.Test
{
    [TestClass]
    public class ReferenceGraphTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            try
            {

                var refObject = MongoConnector.Deserialize("");

                //var result = MongoConnector.GetBaseClassData(refObject, "5a995c1fa0198504f3aaad73", "5a99515da0198504f3aaad70");
                //var asd = result;
            }
            catch(Exception ex)
            {

            }
        }
    }
}
