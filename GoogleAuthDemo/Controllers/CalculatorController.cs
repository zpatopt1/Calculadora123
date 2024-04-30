using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Prometheus;
using System;
using System.Diagnostics;
using System.Data.SqlClient;
using Microsoft.Build.Framework;
using Microsoft.Data.SqlClient;
using GoogleAuthDemo.Models;



namespace GoogleAuthDemo.Controllers
{

    public class CalculatorController : Controller
    {
        // Declare e inicialize uma métrica
       
        private static readonly Counter _sentDataCounter = Metrics.CreateCounter("data_sent_total", "Total number of data sent to the database");
        private static readonly Histogram _sendDataDuration = Metrics.CreateHistogram("data_send_duration_seconds", "Duration of sending data to the database");

        private readonly string _connectionString;
        private readonly ILogger<CalculatorController> _logger;

        // Injeção de Dependência do IConfiguration e ILogger
        public CalculatorController(IConfiguration config, ILogger<CalculatorController> logger)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
            _logger = logger;
        }


        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    
        [HttpPost]
        public IActionResult SendToDataBase([FromBody] ResultModel model)
        {
            double result = model.Result;
            string history = model.History;

                _logger.LogInformation("Dados enviados para teste: {result}", result);
                _logger.LogInformation("Dados enviados para teste: {history}", history);
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    _logger.LogInformation("Dados enviados para teste: {result}", result);
                    _logger.LogInformation("Conta efetuada: {history}", history);
                        
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "INSERT INTO Results (Result, History) VALUES (@Result, @History)";
                        command.Parameters.AddWithValue("@Result", result);
                        command.Parameters.AddWithValue("@History", history);

                        // Medir a duração da operação
                        var timer = _sendDataDuration.WithLabels("insert_result").NewTimer();
                        try
                        {
                            command.ExecuteNonQuery();
                        }
                        finally
                        {
                            timer.ObserveDuration();
                        }
                        
                    }
                    
                    // Aqui você pode salvar o resultado no banco de dados
                    _sentDataCounter.Inc();
                    return Ok("enviado com sucesso ");
                }

                // Retorne uma resposta adequada, por exemplo:
                //_sentDataCounter.Inc();
                //return Ok("Resultado salvo com sucesso na base de dados.");
            }
            catch (Exception ex)
            {
                // Log or handle the exception accordingly
                _logger.LogError(ex, "Erro ao salvar o resultado na base de dados.");
                return StatusCode(500, "Ocorreu um erro ao salvar o resultado na base de dados.");
            }
        }
// [HttpPost]
// public IActionResult SendToDataBase([FromBody] object requestBody)
// {
//     try
//     {
//         _logger.LogInformation("Corpo da solicitação recebido: {requestBody}", requestBody);
        
//         // Converta requestBody para o tipo adequado, se necessário
//         double result = Convert.ToDouble(requestBody); // Exemplo de conversão para double

//         using (var connection = new SqlConnection(_connectionString))
//         {
//             // connection.Open();
//             // _logger.LogInformation("Dados enviados para teste: {result}", result);
//             // // Aqui você pode salvar o resultado no banco de dados
//             return Ok(new { message = "enviado com sucesso" }); // Resposta como objeto JSON
//         } 
//     }
//     catch (Exception ex)
//     {
//         // Log or handle the exception accordingly
//         // _logger.LogError(ex, "Erro ao salvar o resultado na base de dados.");
//         return StatusCode(500, new { error = "Ocorreu um erro ao salvar o resultado na base de dados." }); // Resposta como objeto JSON
//     }
// }

    }
}
