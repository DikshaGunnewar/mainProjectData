using System.Web;
using System.Web.Optimization;

namespace Autopilot
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                       "~/Scripts/jquery-3.1.1.js",
                       "~/Scripts/jquery.unobtrusive-ajax.js",
                       "~/Scripts/bootstrap.js"
               
                       ));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                       "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"
                        ));


            bundles.Add(new StyleBundle("~/bundles/css").Include(
                      "~/Content/bootstrap.css"
                     //"~/Content/bootstrap-datepicker.min.css"
                     ));

            bundles.Add(new ScriptBundle("~/bundles/themesJs").Include(
                "~/vendor/pacejs/pace.min.js",
                "~/vendor/chart.js/dist/Chart.min.js",
                "~/vendor/toastr/toastr.min.js",
                "~/vendor/select2/dist/js/select2.js",
                "~/vendor/moment/moment.js",
                "~/vendor/datetimepicker/bootstrap-datetimepicker.js",
                "~/vendor/datatables/datatables.min.js",                
                "~/Scripts/luna.js"
                ));

            bundles.Add(new StyleBundle("~/bundles/themesCss").Include(
                   "~/vendor/fontawesome/css/font-awesome.css",
                  "~/vendor/select2/dist/css/select2.min.css",
                    "~/vendor/animate.css/animate.css",
                    "~/vendor/bootstrap/css/bootstrap.css",
                   "~/styles/pe-icons/pe-icon-7-stroke.css",
                    "~/styles/pe-icons/helper.css",
                    "~/styles/stroke-icons/style.css",
                    "~/styles/style.css",
                    "~/vendor/toastr/toastr.min.css",
                    "~/vendor/datetimepicker/bootstrap-datetimepicker.css",
                    "~/vendor/datatables/datatables.min.css",
                    "~/styles/local.css"
                ));

      //      bundles.Add(new ScriptBundle("~/vendor")
      //                .IncludeDirectory("~/vendor", "*.js", true));

      //      bundles.Add(new ScriptBundle("~/vendor")
      //               .IncludeDirectory("~/vendor", "*.css", true));


      //      bundles.Add(new StyleBundle("~/bundles/animate.css").Include(
      //               "~/vendor/animate/animate.css"));

      //      bundles.Add(new StyleBundle("~/bundles/bootstrap.css").Include(
      //              "~/vendor/bootstrap/css/bootstrap.css"));

      //      bundles.Add(new StyleBundle("~/bundles/bootstrap.css").Include(
      //              "~/vendor/bootstrap/css/bootstrap.css"));


      //      bundles.Add(new StyleBundle("~/bundles/pe-icon-7-stroke.css").Include(
      //             "~/styles/pe-icons/pe-icon-7-stroke.css"));

      //      bundles.Add(new StyleBundle("~/bundles/pe-icon-7-stroke.css").Include(
      //         "~/styles/pe-icons/helper.css"));


      //      bundles.Add(new StyleBundle("~/bundles/pe-icon-7-stroke.css").Include(
      //"~/styles/pe-icons/helper.css"));

      //      bundles.Add(new StyleBundle("~/bundles/helper.css").Include(
      //       "~/styles/pe-icons/helper.css"));

      //      bundles.Add(new StyleBundle("~/bundles/style.css").Include(
      //             "~/styles/pe-icons/helper.css"));

      //      bundles.Add(new StyleBundle("~/bundles/helper.css").Include(
      //     "~/styles/stroke-icons/style.css"));

      //      bundles.Add(new StyleBundle("~/bundles/style.css").Include(
      //      "~/styles/style.css"));
                
        }
    }
}
