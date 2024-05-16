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


        [Authorize]
        public IActionResult Index()
        {
            return View();
        }



        public bool InternalWait()
        {
            var transaction = Agent.Tracer.CurrentTransaction;
            var span = transaction.StartSpan("tempo", "internal_wait");


            try
            {
              
                Stopwatch stopwatch = Stopwatch.StartNew();

                // Pausa de 2 segundos
                System.Threading.Thread.Sleep(2000);

                stopwatch.Stop();

                // Registrar o tempo de espera
                _logger.LogInformation("Tempo de espera: {milliseconds} milissegundos", stopwatch.ElapsedMilliseconds);

                // Por exemplo, o número de iterações no loop vazio
                int iterations = 0;
                for (int i = 0; i < 100000; i++)
                {
                    iterations++;
                }

                _logger.LogInformation("Número de iteracoes no loop vazio: {iterations}", iterations);

            

                // Retornar true se a espera for concluída com sucesso
                return true;
            }
            catch (Exception ex)
            {
                // Log de erro e retorno de status de erro
                _logger.LogError(ex, "Erro durante a espera interna.");
                return false;
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

            try
            {
                InternalWait();
                InternalWait();

                var transaction = Agent.Tracer.CurrentTransaction;
                var span = transaction.StartSpan("Database Operation", "sql", "sql");

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

                span.End();


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













        /*[HttpPost]
        public IActionResult SendToDataBase([FromBody] ResultModel model)
        {
            double result = model.Result;
            string history = model.History;

            var transaction = Agent.Tracer.CurrentTransaction;

            var span = transaction.StartSpan("Database Operation", "sql", "sql");
            try
            {

                await SendToDataBase();
                return Ok();
            }
            finally
            {
                span.End();
            }

            return StatusCode(500, new { error = "Não foi possível iniciar a transação do APM." });

            try
            {

                // Save to SQL Server using Entity Framework Core
                _context.Results.Add(model);
                _context.SaveChanges();


                // Save to SQL Server using Entity Framework Core

                _logger.LogInformation("Dados enviados para teste: {result}", result);
                _logger.LogInformation("Conta efetuada: {history}", history);

                
                InternalWait();

                _sentDataCounter.Inc();
                return Ok("enviado com sucesso");
               
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar o resultado na base de dados.");
                return StatusCode(500, "Ocorreu um erro ao salvar o resultado na base de dados.");
            }
           
    {*/
        /*   
         *    namespace GoogleAuthDemo.Modelspublic class ResultModel
                {
                    public int Id { get; set; } // Propriedade de identificação única
                    public double Result { get; set; } // Propriedade para armazenar o resultado
                    public string History { get; set; } // Propriedade para armazenar as contas
                                                        // Outras propriedades, se necessário

                    // Se você estiver usando Entity Framework Core, você pode precisar de uma convenção específica
                    // para que o Entity Framework Core reconheça a propriedade de chave primária automaticamente.
                    // Caso contrário, você pode decorar a propriedade Id com a anotação [Key].
                }
        using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
        using Microsoft.EntityFrameworkCore;
        using GoogleAuthDemo.Models;


        namespace GoogleAuthDemo.Data
        {
            public class ApplicationDbContext : IdentityDbContext
            {
                public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                    : base(options)
                {
                }

                public DbSet<ResultModel> Results { get; set; }
            }
        }
            }
        */
        /*       }
           }*/

        /*   public async Task PostDataSql()
           {
               await _context.Database.MigrateAsync();
               var pessoa = new Pessoa
               {
                   DataNascimento = DateTime.Now,
                   Endereco = "Teste teste teste teste teste teste teste teste teste"
               };

               for (var i = 0; i < 200; i++)
               {
                   pessoa.Id = 
                   pessoa.Nome = $"Pessoa teste {i}";
                   _context.Pessoas.Add(pessoa);
               }

               await _context.SaveChangesAsync();

               var pessoas = _context.Pessoas.Where(p => p.Nome.Contains("Pessoa")).ToList();
               var enderecos = _context.Pessoas.Where(p => p.Endereco.Contains("teste")).ToList();
           }

       namespace Sample.ElasticApm.Domain.Model;

       public class ActorsAggregationModel
       {
           public double TotalAge { get; set; }
           public double TotalMovies { get; set; }
           public double AverageAge { get; set; }
       }

        [HttpPost("sql")]
        public async Task<IActionResult> PostSampleSqlAsync()
        {
            await _sampleApplication.PostDataSql();

            return Ok();


        }
        using Microsoft.EntityFrameworkCore;
    using Sample.ElasticApm.Persistence.Entity;

    namespace Sample.ElasticApm.Persistence.Context;

    public class SampleDataContext : DbContext
    {
        public SampleDataContext(DbContextOptions<SampleDataContext> options)
            : base(options)
        {

        }

        public virtual DbSet<Pessoa> Pessoas { get; set; }
    }

       */







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
