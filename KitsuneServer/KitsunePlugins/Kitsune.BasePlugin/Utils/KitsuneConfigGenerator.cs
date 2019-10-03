using Kitsune.BasePlugin.ClientConfigurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kitsune.BasePlugin.Utils
{
    public class BasePluginConfigGenerator
    {
        public static BasePlugin GetBasePlugin(string clientId = null)
        {
            if (!String.IsNullOrEmpty(clientId))
            {
                clientId = clientId.Trim().ToUpper();

                switch (clientId)
                {
                   //Build ClientID specific Overloaded Object (which is inherited from BasePlugin
                   //example - shown below
                    //case Constants.ClientIdConstants.NowFloatsClientId:
                    //    return new NowFloatsClientConfig();

                    default:
                        return new BasePlugin();
                }
            }
            return new BasePlugin();
        }
    }
}