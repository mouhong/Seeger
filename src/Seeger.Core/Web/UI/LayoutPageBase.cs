﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Web.UI.WebControls;

using Seeger.Data;
using Seeger.Caching;
using System.IO;
using System.Web.UI;
using Seeger.Plugins;
using Seeger.Config;
using Seeger.Globalization;
using Seeger.Security;
using System.Web;

namespace Seeger.Web.UI
{
    public abstract class LayoutPageBase : PageBase
    {
        public static readonly string QueryStringParam_PageId = "pageid";

        public int PageId
        {
            get
            {
                return Convert.ToInt32(Request.QueryString[QueryStringParam_PageId]);
            }
        }

        public string Suffix
        {
            get
            {
                return Request.QueryString["suffix"] ?? String.Empty;
            }
        }

        public bool IsInDesignMode
        {
            get
            {
                return Request.QueryString.TryGetValue<bool>("design", false);
            }
        }

        public CultureInfo PageCulture
        {
            get
            {
                CultureInfo culture = null;

                if (IsInDesignMode)
                {
                    var cultureName = (Request.QueryString["culture"] ?? String.Empty).Trim();

                    if (!String.IsNullOrEmpty(cultureName))
                    {
                        culture = CultureInfo.GetCultureInfo(cultureName);
                    }
                }

                if (culture == null)
                {
                    culture = System.Threading.Thread.CurrentThread.CurrentCulture;
                }

                return culture;
            }
        }

        private SEOInfo _seoInfo;
        public SEOInfo SEOInfo
        {
            get
            {
                if (_seoInfo == null)
                {
                    // Try merge page seo settings
                    if (FrontendSettings.Multilingual)
                    {
                        _seoInfo = PageItem.GetSeoInfo(CultureInfo.CurrentCulture, true);

                        var siteInfo = SiteInfoCache.From(NhSession).GetSiteInfo(CultureInfo.CurrentCulture);
                        if (siteInfo != null)
                        {
                            _seoInfo.Merge(siteInfo.SEOInfo);
                        }
                    }
                    else
                    {
                        _seoInfo = PageItem.GetSeoInfo(true);

                        var defaultSiteInfo = GlobalSettingManager.Instance.DefaultSiteInfo;
                        _seoInfo.Merge(defaultSiteInfo.PageTitle, defaultSiteInfo.MetaKeywords, defaultSiteInfo.MetaDescription);
                    }
                }

                return _seoInfo;
            }
        }

        private PageItem _pageItem;
        public PageItem PageItem
        {
            get
            {
                if (_pageItem == null)
                {
                    _pageItem = NhSession.Get<PageItem>(PageId);
                }
                return _pageItem;
            }
        }

        protected AdminSession AdminSession
        {
            get
            {
                return AdminSession.Current;
            }
        }

        public PageResources PageResources
        {
            get
            {
                return PageResources.GetCurrent(Context);
            }
        }

        public IList<StyleResource> Styles
        {
            get
            {
                return PageResources.Styles;
            }
        }

        public IList<ScriptResource> HeadScripts
        {
            get
            {
                return PageResources.HeadScripts;
            }
        }

        public IList<ScriptResource> FootScripts
        {
            get
            {
                return PageResources.FootScripts;
            }
        }

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);

            if (IsInDesignMode)
            {
                if (!AdminSession.IsAuthenticated)
                {
                    AuthenticationService.RedirectToLoginPage();
                }
                else if (!AdminSession.User.IsSuperAdmin && !Authorize(AdminSession.User))
                {
                    AuthenticationService.RedirectToUnauthorizedPage();
                }
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            FixFromActionUrl();

            foreach (var interceptor in PageLifecycleInterceptors.GetEnabledInterceptors())
            {
                interceptor.OnInit(this);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            foreach (var interceptor in PageLifecycleInterceptors.GetEnabledInterceptors())
            {
                interceptor.OnLoad(this);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            SetupSeo();
            IncludeSkinCssFiles();

            if (IsInDesignMode)
            {
                Title = ResourceFolder.Global.GetValue("Common.Designer", CultureInfo.CurrentUICulture) + " (" + PageItem.DisplayName + ")";
                IncludeDesignerElements();
            }
            else if (AdminSession.IsAuthenticated && Authorize(AdminSession.User))
            {
                RegisterDesignerLauncher();
            }

            foreach (var interceptor in PageLifecycleInterceptors.GetEnabledInterceptors())
            {
                interceptor.OnPreRender(this);
            }
        }

        private void IncludeDesignerElements()
        {
            Styles.Add(new StyleResource { Path = AdminSession.Skin.GetFileVirtualPath("page-designer.css") });     
       
            if (AdminSession.Skin.ContainsFile("page-designer.css", AdminSession.UICulture))
            {
                Styles.Add(new StyleResource{ Path = AdminSession.Skin.GetFileVirtualPath("page-designer.css", AdminSession.UICulture) });
            }

            HeadScripts.Add(new ScriptResource { Path = "/Scripts/jquery/jquery.min.js" });
            HeadScripts.Add(new ScriptResource { Path = "/Scripts/jquery/jquery-ui.min.js" });

            if (Form != null)
            {
                Form.Controls.Add(LoadControl("/Admin/Designer/DesignerElement.ascx"));
            }
        }

        private void RegisterDesignerLauncher()
        {
            Styles.Add(new StyleResource { Path = "/Styles/designer-launcher.css" });
            if (Form != null)
            {
                Form.Controls.Add(LoadControl("/Admin/Designer/Launcher.ascx"));
            }
        }

        private bool Authorize(User user)
        {
            return user.HasPermission(null, "PageMgnt", "Design");
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);

            foreach (var interceptor in PageLifecycleInterceptors.GetEnabledInterceptors())
            {
                interceptor.OnUnload(this);
            }
        }

        protected virtual void IncludeSkinCssFiles()
        {
            if (PageItem.Skin != null)
            {
                foreach (var path in PageItem.Skin.GetCssFileVirtualPaths(PageCulture))
                {
                    Styles.Add(new StyleResource { Path = path });
                }
            }
        }

        protected virtual void FixFromActionUrl()
        {
            if (Form != null)
            {
                Form.Action = Request.RawUrl;
            }
        }

        protected virtual void SetupSeo()
        {
            if (IsInDesignMode) return;

            if (!String.IsNullOrEmpty(SEOInfo.PageTitle))
            {
                Title = SEOInfo.PageTitle;
            }
            if (!String.IsNullOrEmpty(SEOInfo.MetaKeywords))
            {
                MetaKeywords = SEOInfo.MetaKeywords;
            }
            if (!String.IsNullOrEmpty(SEOInfo.MetaDescription))
            {
                MetaDescription = SEOInfo.MetaDescription;
            }
        }
    }
}
