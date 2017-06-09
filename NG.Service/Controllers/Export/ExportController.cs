using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using OfficeOpenXml;
using System.Threading.Tasks;
using Microsoft.AspNetCore.NodeServices;

namespace naturalgas.Controllers.Export
{
    [Route("api/export")]
    public class ExportController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        public ExportController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        [Route("DemoExcel")]
        public IActionResult DemoExcel()
        {
            string sWebRootFolder = _hostingEnvironment.WebRootPath;
            string sFileName = @"ExportedDocuments/demo.xlsx";
            string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, sFileName);
            string localFilePath = Path.Combine(sWebRootFolder, sFileName);
            FileInfo file = new FileInfo(localFilePath);
            if (file.Exists)
            {
                file.Delete();
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            }
            using (ExcelPackage package = new ExcelPackage(file))
            {
                // add a new worksheet to the empty workbook
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Employee");
                //First add the headers
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Name";
                worksheet.Cells[1, 3].Value = "Gender";
                worksheet.Cells[1, 4].Value = "Salary (in $)";

                //Add values
                worksheet.Cells["A2"].Value = 1000;
                worksheet.Cells["B2"].Value = "Jon";
                worksheet.Cells["C2"].Value = "M";
                worksheet.Cells["D2"].Value = 5000;

                worksheet.Cells["A3"].Value = 1001;
                worksheet.Cells["B3"].Value = "Graham";
                worksheet.Cells["C3"].Value = "M";
                worksheet.Cells["D3"].Value = 10000;

                worksheet.Cells["A4"].Value = 1002;
                worksheet.Cells["B4"].Value = "Jenny";
                worksheet.Cells["C4"].Value = "F";
                worksheet.Cells["D4"].Value = 5000;

                package.Save(); //Save the workbook.
            }
            FileStream fs = new FileStream(localFilePath, FileMode.Open);
            FileStreamResult fileStreamResult = new FileStreamResult(fs, "application/vnd.ms-excel");
            fileStreamResult.FileDownloadName = "Excel Report.xlsx";
            return fileStreamResult;
        }

        [HttpGet]
        [Route("DemoPDF")]
        public async Task<IActionResult> DemoPDF([FromServices] INodeServices nodeServices)
        {
            var htmlContent = "<h1>Hello From Controller</h1>";
            var result = await nodeServices.InvokeAsync<byte[]>("./pdfReport", htmlContent);
            HttpContext.Response.ContentType = "application/pdf";
            string filename = @"report.pdf";
            HttpContext.Response.Headers.Add("x-filename", filename);
            HttpContext.Response.Headers.Add("Access-Control-Expose-Headers", "x-filename");
            HttpContext.Response.Body.Write(result, 0, result.Length);
            return new ContentResult();
        }
    }
}