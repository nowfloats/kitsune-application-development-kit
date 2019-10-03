using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kitsune.SyntaxParser;
using Microsoft.CSharp;
using Kitsune.Language.Models;

namespace Kitsune.Language.Test
{
    [TestClass]
    public class UnitTest1
    {
        //[TestMethod]
        //public void CreateNowFloatsLanguage()
        //{
        //    var NFLanguage = new NowFloatsLanguage();
        //    Assert.IsNotNull(NFLanguage.LoadThemeDataDelegate);
        //}
        [TestMethod]

        public void TestExpression()    
        {
            var result = Parser.Execute("(2 > 0) && ((4 % 2) > 0)");
            var test = result;
           Assert.AreEqual((int)result, 6);
        }
        [TestMethod]

        public void ValidateExpression()
        {
            var result = Parser.GetObjects("'https://twitter.com/intent/tweet?text=Check+out++now!' + Business.RootAliasUrl.Url + '+Its+worth+the+click.'");
            var test = result;
           // Assert.AreEqual((int)result, 6);
        }
        [TestMethod]
        public void SaveLanguage()
        {
           // var entity = new KEntity();
           // entity.SetEntityType();
           // var service = new LanguageAPI();
           //var result = service.CreateOrUpdateLanguageApi(new API.SDK.Models.CreateOrUpdateLanguageModel { Entity = entity });
        }
    }
}
