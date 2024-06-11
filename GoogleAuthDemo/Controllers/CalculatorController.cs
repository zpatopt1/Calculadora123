using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Prometheus;
using System;
using System.Diagnostics;
using System.Data.SqlClient;
using Microsoft.Build.Framework;
using Microsoft.Data.SqlClient;
using GoogleAuthDemo.Models;
using GoogleAuthDemo.Data;
using Nest;
using Elastic.Apm.Api;
using Elastic.Apm.Api;
using Elastic.Apm;
using GoogleAuthDemo;



namespace GoogleAuthDemo.Controllers
{

    public class CalculatorController : Controller
    {


        private readonly ApplicationDbContext _context;
        private readonly ILogger<CalculatorController> _logger;
        private readonly IElasticClient _elasticClient;
        private static readonly Counter _sentDataCounter = Prometheus.Metrics.CreateCounter("data_sent_total", "Total number of data sent to the database");
        //private static readonly Histogram _sendDataDuration = Prometheus.Metrics.CreateHistogram("data_send_duration_seconds", "Duration of sending data to the database");                                    

        // Injeção de Dependência do IConfiguration e ILogger
        public CalculatorController(ApplicationDbContext context, ILogger<CalculatorController> logger, IElasticClient elasticClient)
        {
            _context = context;
            _logger = logger;
            _elasticClient = elasticClient;

        }


        // [Authorize]
        public IActionResult Index()
        {
            return View();
        }



        public void InternalWait(int number)
        {
            var transaction = Agent.Tracer.CurrentTransaction;
            var span = transaction.StartSpan("Result", "internal_wait");

            try
            {
                // Registrar o número
                _logger.LogInformation("Número fornecido: {number}", number);

                // Adicionar o número como label no span
                span.Context.Labels["number"] = number.ToString();

                // Por exemplo, o número de iterações no loop vazio
                int iterations = 0;
                for (int i = 0; i < 100000; i++)
                {
                    iterations++;
                }

                _logger.LogInformation("Número de iteracoes no loop vazio: {iterations}", iterations);
            }
            catch (Exception ex)
            {
                // Log de erro e retorno de status de erro
                _logger.LogError(ex, "Erro durante a espera interna.");
            }
            finally
            {
                span.End();
            }
        }



        public async Task SendToDataBaseTeste()
        {
            try
            {
                // Verificar se o índice existe no Elasticsearch e criar se não existir
                if (!(await _elasticClient.Indices.ExistsAsync(nameof(ResultModel).ToLower())).Exists)
                    await _elasticClient.Indices.CreateAsync(nameof(ResultModel).ToLower());

                // Obter os dados do modelo de resultados
                var sampleData = ResultModel.GetCalculatorData();

                // Indexar os dados no Elasticsearch
                var response = await _elasticClient.IndexManyAsync(sampleData, nameof(ResultModel).ToLower());

                // Verificar se a operação de indexação foi bem-sucedida
                if (!response.IsValid)
                {
                    // Se a operação não foi bem-sucedida, lançar uma exceção com a mensagem de erro
                    throw new Exception("Erro ao indexar dados no Elasticsearch: " + response.OriginalException?.Message);
                }

                // Incrementar o contador de dados enviados com sucesso
                _sentDataCounter.Inc();
            }
            catch (Exception ex)
            {
                // Em caso de erro, registrar o erro e lançar uma exceção com a mensagem
                _logger.LogError(ex, "Erro durante o envio de dados para o Elasticsearch.");
                throw new Exception("Erro durante o envio de dados para o Elasticsearch: " + ex.Message);
            }
        }


        [HttpPost]
        public async Task<IActionResult> SendToDataBase([FromBody] ResultModel model)
        {
            ITransaction transaction = Agent.Tracer.CurrentTransaction;

            try
            {
                // Registre o número inicialmente
                InternalWait((int)model.Result);

                // Verifique se o resultado é par
                if (model.Result % 2 == 0)
                {
                    // Inicie um span para documentar a espera adicional
                    ISpan waitSpan = transaction.StartSpan("Waiting", "custom");
                    waitSpan.Context.Labels["reason"] = "Result is even, adding delay";
                    waitSpan.Context.Labels["number"] = model.Result.ToString();

                    // Pausa de 2 segundos
                    System.Threading.Thread.Sleep(2000);

                    waitSpan.End();
                }

                // Inicie um span para a operação do banco de dados
                ISpan dbSpan = transaction.StartSpan("Database Operation", "sql", "sql");


                // Salvar os dados no banco de dados
                _context.Results.Add(model);

                // Indexar o modelo no Elasticsearch
                var response = await _elasticClient.IndexAsync(model, idx => idx
                    .Index(nameof(ResultModel).ToLower()) // Nome do índice no Elasticsearch
                    .Id(model.Id)); // Identificador único para o documento no Elasticsearch

                await _context.SaveChangesAsync();

                // Verificar se a operação de indexação foi bem-sucedida
                if (!response.IsValid)
                {
                    // Se a operação não foi bem-sucedida, lançar uma exceção com a mensagem de erro
                    throw new Exception("Erro ao indexar dados no Elasticsearch: " + response.OriginalException?.Message);
                }

                dbSpan.End();

                // Incrementar contador
                _sentDataCounter.Inc();

                return Ok("Dados salvos e indexados com sucesso!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar o resultado na base de dados e indexar no Elasticsearch.");
                return StatusCode(500, "Ocorreu um erro ao salvar o resultado na base de dados e indexar no Elasticsearch.");
            }
        }

    }
}