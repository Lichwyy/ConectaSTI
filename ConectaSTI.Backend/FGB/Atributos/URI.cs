using System.ComponentModel.DataAnnotations;

namespace FGB.Dominio.Atributos
{
    public class UriAttribute : ValidationAttribute
    {
        public UriAttribute()
        {
            base.ErrorMessage = "main.validacoes.uri";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string text = value as string;
            if (string.IsNullOrEmpty(text))
            {
                return ValidationResult.Success;
            }

            if (Uri.TryCreate(text, UriKind.Absolute, out Uri _))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(base.ErrorMessage, new string[1] { validationContext.MemberName });
        }
    }
}
