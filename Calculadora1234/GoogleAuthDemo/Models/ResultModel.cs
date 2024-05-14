namespace GoogleAuthDemo.Models
{
    public class ResultModel
    {
        public int Id { get; set; } // Propriedade de identificação única
        public double Result { get; set; } // Propriedade para armazenar o resultado
        public string History { get; set; } // Propriedade para armazenar as contas
        // Outras propriedades, se necessário
        
        // Se você estiver usando Entity Framework Core, você pode precisar de uma convenção específica
        // para que o Entity Framework Core reconheça a propriedade de chave primária automaticamente.
        // Caso contrário, você pode decorar a propriedade Id com a anotação [Key].
    }
}