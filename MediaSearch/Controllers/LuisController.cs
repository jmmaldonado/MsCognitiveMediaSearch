using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace MediaSearch.Controllers
{
    public class LuisController : Controller
    {
        // GET: Luis
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public string ProcessIndex()
        {
            Thread.Sleep(5000);

            return "Guardia quiere comer ya!";
        }
    }
}