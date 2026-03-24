using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Http;
using WebApplicationSomiod.Utils;
using ModelsLibrary;
using WebApplicationSomiod.Models;

namespace WebApplicationSomiod.Controllers
{
    [RoutePrefix("api/seguranca")]
    public class SegurancaController : ApiController
    {
        string connStr = Properties.Settings.Default.ConnectionStr;

        [HttpGet, Route("")]
        public IHttpActionResult GetSeguranca()
        {
            return Ok("API de Seguranca ativa");
        }
    }
}
