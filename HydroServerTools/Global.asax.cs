﻿using jQuery.DataTables.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

using System.Collections.Concurrent;

using HydroServerTools.Utilities;
using HydroServerTools.Validators;

using HydroServerToolsUtilities;

namespace HydroServerTools
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static Guid InstanceGuid { get; set;}
        
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Lets MVC know that anytime there is a JQueryDataTablesModel as a parameter in an action to use the
            // JQueryDataTablesModelBinder when binding the model.
            ModelBinders.Binders.Add(typeof(JQueryDataTablesModel), new JQueryDataTablesModelBinder());
            //InstanceGuid = Guid.NewGuid();

            //At web application startup - add ConcurrentDictionary instances to the runtime cache for the following context types:
            //  - file, 
            //  - validation
            //  - repository
            //  - status
            //  - db load
            //
            //NOTE: One HttpRuntime.Cache instance exists for the Application Domain...
            //Sources: https://docs.microsoft.com/en-us/aspnet/web-forms/overview/data-access/caching-data/caching-data-at-application-startup-cs
            //         https://stackoverflow.com/questions/27575213/how-to-cache-in-asp-net
            var cache = HttpRuntime.Cache;
            var key = "uploadIdsToFileContexts";

            ConcurrentDictionary<string, FileContext> filecontexts = new ConcurrentDictionary<string, FileContext>();
            cache.Insert(key, filecontexts);

            key = "uploadIdsToValidationContexts";
            ConcurrentDictionary<string, ValidationContext<CsvValidator>> validationcontexts = new ConcurrentDictionary<string, ValidationContext<CsvValidator>>();
            cache.Insert(key, validationcontexts);

            key = "uploadIdsToRepositoryContexts";
            ConcurrentDictionary<string, RepositoryContext> repositorycontexts = new ConcurrentDictionary<string, RepositoryContext>();
            cache.Insert(key, repositorycontexts);

            key = "uploadIdsToStatusContexts";
            ConcurrentDictionary<string, StatusContext> statuscontexts = new ConcurrentDictionary<string, StatusContext>();
            cache.Insert(key, statuscontexts);

            key = "uploadIdsToDbLoadContexts";
            ConcurrentDictionary<string, DbLoadContext> dbloadcontexts = new ConcurrentDictionary<string, DbLoadContext>();
            cache.Insert(key, dbloadcontexts);


        }

        //Disable forms authentication redirect...
        //Source: https://stackoverflow.com/questions/18519446/prevent-asp-net-from-redirecting-to-login-aspx
        protected void Application_EndRequest(Object sender, EventArgs eventArgs)
        {
            HttpApplication context = (HttpApplication)sender;
            context.Response.SuppressFormsAuthenticationRedirect = true;
        }

        void Session_Start(object sender, EventArgs e)
        {
            // Code that runs when a new session is started  
            


        }

        void Session_End(object sender, EventArgs e)
        {
            // Code that runs when a session ends.   
            // Note: The Session_End event is raised only when the sessionstate mode  
            // is set to InProc in the Web.config file. If session mode is set to StateServer   
            // or SQLServer, the event is not raised. 
            

        }
    }
}
