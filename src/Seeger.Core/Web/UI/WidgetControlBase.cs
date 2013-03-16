﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Seeger.Caching;
using Seeger.Data;
using Seeger.Plugins.Widgets;
using Seeger.Plugins;
using System.Globalization;

namespace Seeger.Web.UI
{
    public abstract class WidgetControlBase : UserControlBase
    {
        public bool IsInDesignMode { get; internal set; }

        public WidgetDefinition Widget { get; internal set; }

        public LocatedWidget LocatedWidget { get; internal set; }

        public EntityAttributeCollection WidgetAttributes { get; private set; }

        public int PageId
        {
            get
            {
                return Request.QueryString.TryGetValue<int>("pageId", 0);
            }
        }

        public string Suffix
        {
            get
            {
                return Request.QueryString["suffix"];
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

        protected WidgetControlBase()
        {
            WidgetAttributes = new EntityAttributeCollection();
        }

        public override void RenderControl(System.Web.UI.HtmlTextWriter writer)
        {
            if (IsInDesignMode)
            {
                writer.AddAttribute("class", "sig-widget");
                writer.AddAttribute("plugin-name", Widget.Plugin.Name);
                writer.AddAttribute("widget-name", Widget.Name);
                writer.AddAttribute("editable", Widget.Editable.ToString().ToLower());
                writer.AddAttribute("editor-width", Widget.EditorSettings.Width.ToString());
                writer.AddAttribute("editor-height", Widget.EditorSettings.Height.ToString());

                if (LocatedWidget != null)
                {
                    writer.AddAttribute("widget-in-page-id", LocatedWidget.Id.ToString());
                    writer.AddAttribute("order", LocatedWidget.Order.ToString());
                }

                writer.RenderBeginTag("div");
            }

            base.RenderControl(writer);

            if (IsInDesignMode)
            {
                writer.RenderEndTag();
            }
        }
    }
}
