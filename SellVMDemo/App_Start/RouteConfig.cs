﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SellVMDemo
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Bill", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
            "avaliableSizes",                                              
            "{controller}/{action}/{tenantID}/{subID}/{region}",                           
            new { controller = "Vmoptions", action = "avaliable", tenantID = "", subID = "" ,region="" }  
            );
        }
    }
}
