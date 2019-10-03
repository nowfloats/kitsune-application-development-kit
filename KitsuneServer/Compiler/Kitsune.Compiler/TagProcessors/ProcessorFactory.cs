using Kitsune.Language.Helper;
using Kitsune.Language.Models;
using System;
using System.Collections.Generic;

namespace Kitsune.Compiler.TagProcessors
{
    public static class ProcessorFactory
    {
        static Dictionary<string, Processor> ProcessorDictionary;
        static Processor noOpProcessor;

        static void InitializeFactory() {
            ProcessorDictionary = new Dictionary<string, Processor>();
            ProcessorDictionary.Add(LanguageAttributes.KObject.GetDescription().ToLower(), new KObjectProcessor());
            ProcessorDictionary.Add(LanguageAttributes.KDL.GetDescription().ToLower(), new KDLProcessor());
            ProcessorDictionary.Add(LanguageAttributes.KRepeat.GetDescription().ToLower(), new KRepeatProcessor());
            ProcessorDictionary.Add(LanguageAttributes.KScript.GetDescription().ToLower(), new KScriptProcessor());
            ProcessorDictionary.Add(LanguageAttributes.KSearch.GetDescription().ToLower(), new KSearchProcessor());
            noOpProcessor = new NoOp();
        }

        public static Processor GetProcessor(object p)
        {
            throw new NotImplementedException();
        }

        public static Processor GetProcessor(string ktag)
        {
            if (null == ProcessorDictionary)
            {
                InitializeFactory();
            }

            if (ProcessorDictionary.ContainsKey(ktag))
            {
                return ProcessorDictionary[ktag];
            }
            else
            {
                return noOpProcessor;
            }
        }
    }
}
