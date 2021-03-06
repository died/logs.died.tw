﻿using System.Web.Optimization;

namespace Mvc4
{
    public class BundleConfig
    {
        // 如需 Bundling 的詳細資訊，請造訪 http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/bahamutday").Include(
                "~/Scripts/jqwidgets/jqxcore.js",
                "~/Scripts/jqwidgets/jqxbuttons.js",
                "~/Scripts/jqwidgets/jqxscrollbar.js",
                "~/Scripts/jqwidgets/jqxmenu.js",
                "~/Scripts/jqwidgets/jqxgrid.js",
                "~/Scripts/jqwidgets/jqxgrid.selection.js",
                "~/Scripts/jqwidgets/jqxgrid.columnsresize.js",
                "~/Scripts/jqwidgets/jqxdata.js",
                "~/Scripts/jquery.timelinr-0.9.51.js",
                "~/Scripts/gettheme.js"));

            bundles.Add(new ScriptBundle("~/bundles/bahamutgame").Include(
               "~/Scripts/hichart/highcharts.js",
               "~/Scripts/hichart/modules/exporting.js",
               "~/Scripts/hichart/themes/gray.js",
               "~/Scripts/multiselect/jquery.multi-select.js",
               "~/Scripts/jquery.quicksearch.js"));

            // 使用開發版本的 Modernizr 進行開發並學習。然後，當您
            // 準備好實際執行時，請使用 http://modernizr.com 上的建置工具，只選擇您需要的測試。
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css"));

            bundles.Add(new StyleBundle("~/Content/themes/jqwidgets").Include(
                    "~/Scripts/jqwidgets/styles/jqx.base.css"));

            bundles.Add(new StyleBundle("~/Content/multiselect").Include(
                     "~/Scripts/multiselect/multi-select.css"));
        }
    }
}