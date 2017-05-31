﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cofoundry.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Cofoundry.Web
{
    /// <summary>
    /// Main controller for handling Cofoundry page routing, redirection and not found errors. This route
    /// is configured last so all other controller routes are scanned first before falling back to this. 
    /// </summary>
    public class CofoundryPagesController : Controller
    {
        private readonly ICultureContextService _cultureContextService;
        private readonly IPageLocaleParser _pageLocaleParser;
        private readonly IPageActionRoutingStepFactory _pageActionRoutingStepFactory;

        private ActiveLocale _locale;

        #region Constructor

        public CofoundryPagesController(
            ICultureContextService cultureContextService,
            IPageLocaleParser pageLocaleParser,
            IPageActionRoutingStepFactory pageActionRoutingStepFactory
            )
        {
            _cultureContextService = cultureContextService;
            _pageLocaleParser = pageLocaleParser;
            _pageActionRoutingStepFactory = pageActionRoutingStepFactory;
            
        }

        #endregion

        #region controller lifecycle 

        //protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        //{
        //    base.Initialize(requestContext);

        //    // Do some pre-init before the page action executes, to find the locale
        //    if ((string)requestContext.RouteData.Values["action"] == "Page")
        //    {
        //        string path = (string)requestContext.RouteData.Values["path"];

        //        _locale = _pageLocaleParser.ParseLocale(path);
        //        if (_locale != null)
        //        {
        //            _cultureContextService.SetCurrent(_locale.IETFLanguageTag);
        //        }
        //    }
        //}

        #endregion

        #region main page route
        
        public async Task<IActionResult> Page(
            string path, 
            string mode, 
            int? version = null,
            string editType = "entity"
            )
        {
            // Init state
            var state = new PageActionRoutingState();
            state.Locale = _locale;
            state.InputParameters = new PageActionInputParameters()
            {
                Path = path,
                VisualEditorMode = mode,
                VersionId = version,
                IsEditingCustomEntity = editType == "entity"
            };

            // Run through the pipline in order
            foreach (var method in _pageActionRoutingStepFactory.Create())
            {
                await method.ExecuteAsync(this, state);
                // If we get an action result, do an early return
                if (state.Result != null)
                {
                    return state.Result;
                }
            }

            // We should never get here!
            throw new InvalidOperationException("Unknown Page Routing State");
        }
        
        #endregion
    }
}