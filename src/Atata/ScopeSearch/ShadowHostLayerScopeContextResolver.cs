﻿using System.Collections.ObjectModel;
using System.Linq;
using OpenQA.Selenium;

namespace Atata
{
    public class ShadowHostLayerScopeContextResolver : ILayerScopeContextResolver
    {
        private const string GetShadowRootChildElementsScript =
@"var shadowChildren = arguments[0].shadowRoot.children;
var filteredChildren = [];

for (var i = 0; i < shadowChildren.length; i++) {
    var nodeName = shadowChildren[i].nodeName;

    if (nodeName !== 'STYLE' && nodeName !== 'SCRIPT') {
        filteredChildren.push(shadowChildren[i]);
    }
}

return filteredChildren;";

        public string DefaultOuterXPath => "..//";

        public ISearchContext Resolve(IWebElement element)
        {
            var shadowChildren = (ReadOnlyCollection<IWebElement>)AtataContext.Current.Driver.ExecuteScript(
                GetShadowRootChildElementsScript,
                element);

            return shadowChildren.First();
        }
    }
}
