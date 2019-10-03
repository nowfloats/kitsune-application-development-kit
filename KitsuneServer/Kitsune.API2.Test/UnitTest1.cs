using Kitsune.API2.Validators;
using Kitsune.Compiler.Helpers;
using Kitsune.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Kitsune.API2.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var asd = "asd";

            ///ProjectConfigValidator asad = new ProjectConfigValidator();
        }

        [TestMethod]
        public void TestMethod2()
        {
            var Validator = ProjectConfigValidator.GetInstance(new Uri("https://s3.ap-south-1.amazonaws.com/kitsune-buildtest-resources/schema.json"));
            Validator.ValidateJson("{'payments':'ad'}");
        }

        [TestMethod]
        public void EncryptDecryptFunction()
        {
            string message = "Hello my name is Rahul Kedia";
            string key = "secret";
            string encryptedValue = new EncryptDecryptHelper(key).Encrypt(message,true);
            string decryptedValue = new EncryptDecryptHelper(key).Decrypt(encryptedValue, true);
            Assert.AreEqual(decryptedValue, message);
        }

        [TestMethod]
        public void EncryptDecryptDateTimeFunction()
        {
            string WebsiteId = "5ad9c409889084051f87a4b8";
            string key = "secret";

            string dateTime = DateTime.Now.ToUniversalTime().ToString();
            string message = $"{{WebsiteId : \" {WebsiteId} \", DateTime : \" {dateTime} \"}}";
            string encryptedValue = new EncryptDecryptHelper(key).Encrypt(message, true);
            string decryptedValue = new EncryptDecryptHelper(key).Decrypt(encryptedValue, true);
            Assert.AreEqual(decryptedValue, message);
        }
    }
}
