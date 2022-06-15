using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels.Accounts
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "O campo nome é obrigatório")]
        public string Name { get; set; }

        [Required(ErrorMessage = "O campo e-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        public string Email { get; set; }
    }
}
