﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core;
using Wyam.Modules.Razor.Microsoft.AspNet.Mvc;
using Wyam.Modules.Razor.Microsoft.AspNet.Mvc.Rendering;
using Wyam.Modules.Razor.Microsoft.Framework.Internal;

namespace Wyam.Modules.Razor
{
    // Similar convention to ASP.NET MVC HtmlHelper (but totally different class, existing extensions won't work)
    public class HtmlHelper
    {
        private readonly ViewContext _viewContext;

        public HtmlHelper(ViewContext viewContext)
        {
            _viewContext = viewContext;
        }

        public HtmlString Raw(string value)
        {
            return new HtmlString(value);
        }

        public HtmlString Raw(object value)
        {
            return new HtmlString(value == null ? (string)null : value.ToString());
        }

        // Partial support from HtmlHelperPartialExtensions.cs

        public HtmlString Partial(string partialViewName)
        {
            using (var writer = new StringCollectionTextWriter(Encoding.UTF8))
            {
                RenderPartialCore(partialViewName, writer);
                return new HtmlString(writer);
            }
        }

        public void RenderPartial(string partialViewName)
        {
            RenderPartialCore(partialViewName, _viewContext.Writer);
        }

        private void RenderPartialCore(string partialViewName, TextWriter textWriter)
        {
            var viewEngineResult = _viewContext.ViewEngine.FindPartialView(_viewContext, partialViewName);
            if (!viewEngineResult.Success)
            {
                var locations = string.Empty;
                if (viewEngineResult.SearchedLocations != null)
                {
                    locations = Environment.NewLine +
                        string.Join(Environment.NewLine, viewEngineResult.SearchedLocations);
                }

                throw new InvalidOperationException(string.Format("Partial view {0} not found in {1}.", partialViewName, locations));
            }

            var view = viewEngineResult.View;
            using (view as IDisposable)
            {
                //var viewContext = new ViewContext(ViewContext, view, newViewData, writer);
                var viewContext = new ViewContext(view, _viewContext.ViewData, textWriter, _viewContext.ViewEngine);
                AsyncHelper.RunSync(() => viewEngineResult.View.RenderAsync(viewContext));
            }
        }
    }
}
