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





namespace GoogleAuthDemo.Controllers
{

    public class CalculatorController : Controller
    {
    
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CalculatorController> _logger;
        private readonly IElasticClient _elasticClient;

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
    
        [HttpPost]
        public IActionResult SendToDataBase([FromBody] ResultModel model)
        {
            double result = model.Result;
            string history = model.History;

         try
            {
                // Save to SQL Server using Entity Framework Core
     
                _logger.LogInformation("Dados enviados para teste: {result}", result);
                _logger.LogInformation("Conta efetuada: {history}", history);

                return Ok("enviado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar o resultado na base de dados.");
                return StatusCode(500, "Ocorreu um erro ao salvar o resultado na base de dados.");
            }
           
    {
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
            }
        }

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

