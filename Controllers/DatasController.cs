using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NLog;
using NLog.Web;
using System.Text.Json;
using WebApplication1.Models;

namespace WebAPIApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatasController : ControllerBase
    {
        private IConfiguration Configuration { get; set; }
        private Logger Logger { get; set; } = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        public DatasController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // GET api/datas
        [HttpGet]
        public ActionResult<IEnumerable<Data>> Get(bool codeInAscendingOrder, int maxCodeVal)
        {
            try
            {
                List<Data> datas = new();
                string param1 = maxCodeVal > 0 ? $" where Code < '{maxCodeVal}'" : "";
                string param2 = codeInAscendingOrder ? " ORDER BY Code" : "";
                string connStr = Configuration.GetSection("ConnectionStrings:DefaultConnection").Value;

                using SqlConnection conn = new(connStr);
                conn.Open();
                SqlCommand command = new($"Select * from Datas{param1}{param2}", conn);
                Logger.Info(command.CommandText);
                SqlDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    datas.Add(new Data(Convert.ToInt32(dataReader["Id"]), Convert.ToInt32(dataReader["Code"]), dataReader["Value"].ToString() ?? ""));
                    Logger.Info($"{Convert.ToInt32(dataReader["Id"])}, {Convert.ToInt32(dataReader["Code"])}, {dataReader["Value"]}");
                }

                return Ok(datas);
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка получения данных из БД. {ex.Message}");
                return Problem();
            }
        }


        // POST api/datas
        /*
         CREATE TABLE [dbo].[Datas] (
                [Id]    INT            IDENTITY (1, 1) NOT NULL,
                [Code]  INT            NOT NULL,
                [Value] NVARCHAR (MAX) NOT NULL,
                CONSTRAINT [PK_Datas] PRIMARY KEY CLUSTERED ([Id] ASC)
            );
         */
        [HttpPost]
        public ActionResult<Data> Post(JsonElement jstring)
        {
            try
            {
                List<Data> datas = new();
                var elements = jstring.EnumerateArray();
                int i = 0;
                while (elements.MoveNext())
                {
                    var data = elements.Current;
                    var props = data.EnumerateObject();

                    while (props.MoveNext())
                    {
                        try
                        {
                            var prop = props.Current;
                            datas.Add(new Data(i++, int.Parse(prop.Name), prop.Value.ToString()));
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }

                var values = datas.OrderBy(p => p.Code).ToList();
                if (values.Count > 0)
                {
                    string connStr = Configuration.GetSection("ConnectionStrings:DefaultConnection").Value;
                    using SqlConnection conn = new(connStr);
                    conn.Open();
                    SqlCommand deleteCommand = new("Delete from Datas", conn);
                    deleteCommand.ExecuteNonQuery();
                    Logger.Info(deleteCommand.CommandText);

                    string valuesStr = "";
                    foreach (var record in values)
                    {
                        valuesStr += $"({record.Code}, '{record.Value}'),";
                    }
                    SqlCommand insertCommand = new($"Insert into Datas (Code, Value) values {valuesStr.TrimEnd(',')}", conn);
                    insertCommand.ExecuteNonQuery();

                    conn.Close();
                    Logger.Info(insertCommand.CommandText);
                }
                return Ok(values);
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка сохранения данных в БД. {ex.Message}");
                return Problem();
            }
        }
    }
}