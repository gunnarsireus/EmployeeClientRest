using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Mvc;
using MvcClient.Models;

namespace MvcClient.Controllers
{
    public class EmployeeController : Controller
    {
        static string serverAddress = System.Configuration.ConfigurationManager.AppSettings["IISserverAddress"];
        private static T GetData<T>(string order)
        {
            WebClient proxy = new WebClient();
            //byte[] data = proxy.DownloadData(@"http://localhost:8085/EmployeeService/" + order);
            byte[] data = proxy.DownloadData(serverAddress + order);
            Stream stream = new MemoryStream(data);
            DataContractJsonSerializer obj = new DataContractJsonSerializer(typeof(T));
            if (stream.Length > 0)
            {
                return (T)obj.ReadObject(stream);
            }
            return default(T);
        }

        private static void PostData<T>(string order, T body)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream mem = new MemoryStream();
            ser.WriteObject(mem, body);
            string content = Encoding.UTF8.GetString(mem.ToArray(), 0, (int)mem.Length);
            WebClient webClient = new WebClient();
            webClient.Headers["Content-type"] = "application/json";
            webClient.Encoding = Encoding.UTF8;
            //webClient.UploadString("http://localhost:8085/EmployeeService/" + order, "POST", content);
            webClient.UploadString(serverAddress + order, "POST", content);
        }

        // GET: Employee
        public ActionResult Index()
        {
            ViewBag.Connection = serverAddress;
            List<Employee> employees = GetData<List<Employee>>("List");
            return View(employees.OrderBy(o => o.Id).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(IList<Employee> employees)
        {
            ViewBag.Connection = serverAddress;
            if (employees == null)
            {
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                GetData<Employee>("Delete/0");  //If employees with Id=0 exists in db, delete them;
                foreach (var employee in employees)
                {
                    if (employee.Id == 0)
                    {
                        continue;  //If employee with Id=0 has been entered in the view, ignore he/she, will not be stored in db and will disapear in next page load
                    }

                    Employee emp = GetData<Employee>("Get/" + employee.Id.ToString()); //Check if employee already exists in Db

                    if (emp == null) 
                    {
                        PostData("Create", employee); //employee doesn't exist in Db, Create new
                    }
                    else
                    {   
                        PostData("Update", employee);  //employee do exist in Db, Update                     
                    }
                }
            }
            return RedirectToAction("Index");
        }

        // GET: Employee/Create
        public ActionResult Create()
        {
            try
            {
                GetData<Employee>("Add");
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }

        public ActionResult Delete(int id)
        {
            try
            {
                GetData<Employee>("Delete/" + id.ToString());
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }
    }
}
