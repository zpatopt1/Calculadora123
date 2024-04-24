using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace GoogleAuthDemo.Controllers
{

    public class CalculatorController : Controller
    {
        
        private readonly ILogger<CalculatorController> _logger;

          public CalculatorController(ILogger<CalculatorController> logger)
        {
            _logger = logger;
        }
        [Authorize]
        public IActionResult Index()
        {   
                     _logger.LogInformation("teste13131");
            return View();
            
        }

        [HttpPost]
        public IActionResult Calculate(string expression)
        {
            try
            {
                // Lógica de cálculo aqui
                // Supondo que você tenha um método para calcular a expressão
                var result = CalculateExpression(expression);
                ViewBag.Result = result;
                 _logger.LogInformation("Cálculo realizado com sucesso para a expressão: {expression}", expression);
            }
            catch (Exception ex)
            {
                // Lidar com erros de cálculo
                ViewBag.ErrorMessage = "Erro ao calcular a expressão: " + ex.Message;
                 // Exemplo de logging de erro
                _logger.LogError(ex, "Erro ao calcular a expressão: {expression}", expression);
            }

            return View("Index");
        }

        // Método fictício para calcular a expressão
        private double CalculateExpression(string expression)
        {
            // Lógica de cálculo da expressão
            return 0; // Altere isso para retornar o resultado real
        }
    }
}
