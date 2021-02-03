using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;

namespace FunctionApp1
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;


            /*using (PowerShell ps = PowerShell.Create())
            {
                // specify the script code to run.
                ps.AddScript("Set-Item WSMan:localhost\\client\\trustedhosts -value *");

                // specify the parameters to pass into the script.
                //ps.AddParameters();

                // execute the script and await the result.
                var pipelineObjects = await ps.InvokeAsync().ConfigureAwait(false);

                // print the resulting pipeline objects to the console.
                foreach (var item in pipelineObjects)
                {
                    Console.WriteLine(item.BaseObject.ToString());
                }
            }*/

            WSManConnectionInfo connectionInfo = new WSManConnectionInfo();
            connectionInfo.Credential = new PSCredential("Thomas", ConvertToSecureString("Pa$$w0rd1234"));
            connectionInfo.ComputerName = "51.124.30.11";
            Runspace runspace = RunspaceFactory.CreateRunspace(connectionInfo);
            runspace.Open();

            using (PowerShell ps = PowerShell.Create())
            {
                ps.Runspace = runspace;
                string myString = "C:\\Users\\Thomas\\Powershell\\test.ps1";
                Console.WriteLine(myString);

                // specify the script code to run.

                //ps.AddScript("echo " + name);
                ps.AddScript(myString);
                //ps.AddScript("Get-Module");
                //ps.AddScript("Out-String");

                // specify the parameters to pass into the script.
                //ps.AddParameters();

                // execute the script and await the result.
                
                var pipelineObjects = await ps.InvokeAsync().ConfigureAwait(true);

                // print the resulting pipeline objects to the console.
                foreach (var item in pipelineObjects)
                {
                    Console.WriteLine("*****");
                    Console.WriteLine(item.BaseObject.ToString());
                }
            }
            runspace.Close();

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);

            SecureString ConvertToSecureString(string password)
            {
                if (password == null)
                    throw new ArgumentNullException("password");

                var securePassword = new SecureString();

                foreach (char c in password)
                    securePassword.AppendChar(c);

                securePassword.MakeReadOnly();
                return securePassword;
            }
        }
    }
}
