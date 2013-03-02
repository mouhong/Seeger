﻿using Seeger.Web.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;

namespace Seeger.Web
{
    public enum HandlerAuthMode
    {
        AuthByCookie = 0,
        AuthByPostedToken = 1
    }

    public abstract class AuthRequiredHttpHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public virtual string AuthTokenName
        {
            get
            {
                return "AspNetAuth";
            }
        }

        public virtual HandlerAuthMode AuthMode
        {
            get
            {
                return HandlerAuthMode.AuthByCookie;
            }
        }

        public AdministrationSession AdministrationSession { get; private set; }

        public void ProcessRequest(HttpContext context)
        {
            AdministrationSession = Authenticate(context);

            if (AdministrationSession.IsAuthenticated && (AdministrationSession.User.IsSuperAdmin) || ValidateAccess(context, AdministrationSession))
            {
                DoProcessRequest(context);
            }
            else
            {
                OnAccessDenied(context);
            }
        }

        private AdministrationSession Authenticate(HttpContext context)
        {
            if (AuthMode == HandlerAuthMode.AuthByCookie)
            {
                return AdministrationSession.Current;
            }

            var token = context.Request.Params[AuthTokenName];

            if (token != null)
            {
                var ticket = FormsAuthentication.Decrypt(token);

                if (ticket != null)
                {
                    var identity = new FormsIdentity(ticket);
                    var principal = new GenericPrincipal(identity, new string[] { });
                    context.User = principal;
                }
            }

            var user = AuthenticationService.GetCurrentUserFrom(context.User);

            return new AdministrationSession(user);
        }

        protected virtual bool ValidateAccess(HttpContext context, AdministrationSession adminSession)
        {
            return adminSession.IsAuthenticated;
        }

        protected abstract void DoProcessRequest(HttpContext context);

        protected virtual void OnAccessDenied(HttpContext context)
        {
            context.Response.StatusCode = 403;
        }
    }
}
