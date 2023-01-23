using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using TesteConsultaCep.Data;
using TesteConsultaCep.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using MySqlConnector;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.ConstrainedExecution;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using NuGet.Protocol;

namespace TesteConsultaCep.Controllers
{
    public class CepBDsController : Controller
    {
        private readonly TesteConsultaCepContext _context;
        public string _cep;
        
        public CepBDsController(TesteConsultaCepContext context)
        {
            _context = context;
        }

        // GET: CepBDs
        public async Task<IActionResult> Index()
        {
            return View();
        }

        // GET: CepBDs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var Cep = TempData["Cep"];
            var logradouro = await _context.CepBD
                .FirstOrDefaultAsync(m => m.CEP == Cep.ToString());
            
            if (logradouro == null)
            {
                return NotFound();
            }
            return View(logradouro);
        }
        public async Task<IActionResult> Consulta(String Cep)
        {
            _cep = Cep;
            if (Cep == null)
            {
                TempData["Resultado"] = "Cep inválido";
                return RedirectToAction("Index");
            }

            if (Cep.Length > 7 && Cep.Length < 9)
            {
                JObject jsonRetorno = ConsultaViaCep(Cep);
                if (jsonRetorno.Count <= 1)
                {
                    TempData["Resultado"] = "Cep inválido";
                    return RedirectToAction("Index");
                }
                ConsultaCepNaBase(Cep);
            }
            else
            {
                TempData["Resultado"] = "Cep inválido";
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        public JObject ConsultaViaCep(String Cep)
        {
            string result = string.Empty;
            string viaCEPUrl = "https://viacep.com.br/ws/" + Cep + "/json/";
            WebClient client = new WebClient();
            result = client.DownloadString(viaCEPUrl);
            JObject jsonRetorno = JsonConvert.DeserializeObject<JObject>(result);
            return jsonRetorno;
        }

        public void ConsultaCepNaBase(string Cep)
        {
            _cep = Cep;
            string query = $"SELECT Id FROM CEPBD WHERE CEP = {Cep}";
            string resposta = "";

            MySqlConnection connection = new MySqlConnection("server=localhost;userid=developer;password=1234567;database=ConsultaCepContext");
            MySqlCommand MysqlCommand = new MySqlCommand(query, connection);
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            DataSet ds = new DataSet();

            DataView dv;
            MysqlCommand.CommandType = CommandType.Text;

            try
            {
                connection.Open();
                adapter.SelectCommand = MysqlCommand;
                adapter.Fill(ds, "Create DataView");
                adapter.Dispose();

                dv = ds.Tables[0].DefaultView;

                if (dv.Count > 0)
                {
                    TempData["VerificaCepExisteNaBase"] = false;
                    TempData["valorDoCep"] = Cep;
                }
                else
                {
                    TempData["VerificaCepExisteNaBase"] = true;
                    TempData["valorDoCep"] = Cep;
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                connection.Close();
                MysqlCommand.Dispose();
                connection.Dispose();
            }
        }

        public void ExecutaQuery(string query)
        {
            MySqlConnection connection = new MySqlConnection("server=localhost;userid=developer;password=1234567;database=ConsultaCepContext");
            MySqlCommand MysqlCommand = new MySqlCommand(query, connection);

            MysqlCommand.CommandType = CommandType.Text;

            try
            {
                connection.Open();
                MysqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                MysqlCommand.Dispose();
                connection.Close();
                connection.Dispose();
            }


        }

        // GET: CepBDs/Create
        public async Task<IActionResult> Create()
        {
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> NovoLogradouro(string? id)
        {
            string Cep = id;
            _cep = Cep;

            //string result = string.Empty;
            //string viaCEPUrl = "https://viacep.com.br/ws/" + Cep + "/json/";
            //WebClient client = new WebClient();
            //result = client.DownloadString(viaCEPUrl);
            //JObject jsonRetorno = JsonConvert.DeserializeObject<JObject>(result);


            JObject jsonRetorno = ConsultaViaCep(Cep);
            string[] CepFormatado = jsonRetorno["cep"].ToString().Split('-');
            string CEP = CepFormatado[0] + CepFormatado[1];

            if (jsonRetorno["unidade"] == null)
                jsonRetorno["unidade"] = 0;

            string query = "INSERT INTO CEPBD (CEP, Logradouro, Complemento, Bairro, Localidade, Uf, Unidade, Ibge, Gia) VALUES (";
            query = query + "'" + CEP + "'";
            query = query + ",'" + jsonRetorno["logradouro"] + "'";
            query = query + ",'" + jsonRetorno["complemento"] + "'";
            query = query + ",'" + jsonRetorno["bairro"] + "'";
            query = query + ",'" + jsonRetorno["localidade"] + "'";
            query = query + ",'" + jsonRetorno["uf"] + "'";
            query = query + ",'" + jsonRetorno["unidade"] + "'";
            query = query + ",'" + jsonRetorno["ibge"] + "'";
            query = query + ",'" + jsonRetorno["gia"] + "'" + ")";

            ExecutaQuery(query);

            TempData["Cep"] = Cep;
            return RedirectToAction("Details");
        }

        // POST: CepBDs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CepBD cepBD)
        {
            string Cep = _cep;
            JObject jsonRetorno = ConsultaViaCep(Cep);
            string[] CepFormatado = jsonRetorno["cep"].ToString().Split('-');
            string CEP = CepFormatado[0] + CepFormatado[1];

            string query = "INSERT INTO CEPBD (CEP, Logradouro, Complemento, Bairro, Localidade, Uf, Unidade, Ibge, Gia) VALUES (";
            query = query + "'" + CEP + "'";
            query = query + ",'" + jsonRetorno["logradouro"] + "'";
            query = query + ",'" + jsonRetorno["complemento"] + "'";
            query = query + ",'" + jsonRetorno["bairro"] + "'";
            query = query + ",'" + jsonRetorno["localidade"] + "'";
            query = query + ",'" + jsonRetorno["uf"] + "'";
            query = query + ",'" + jsonRetorno["unidade"] + "'";
            query = query + ",'" + jsonRetorno["ibge"] + "'";
            query = query + ",'" + jsonRetorno["gia"] + "'" + ")";
            return RedirectToAction("Details");
        }
    }
}
