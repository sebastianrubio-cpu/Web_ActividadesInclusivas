using System.Net;
using System.Net.Mail;

namespace SistemaWeb.Services
{
    public class CorreoService
    {
        // NOTA: Para que esto funcione con Gmail, debes usar una "Contraseña de Aplicación".
        // Ve a tu cuenta Google -> Seguridad -> Verificación en 2 pasos -> Contraseñas de aplicaciones.
        private readonly string _correoOrigen = "correo_aqui"; // <--- PON TU CORREO AQUÍ
        private readonly string _claveOrigen = "clave_aqui\r\n"; // <--- PON TU CLAVE DE APLICACIÓN AQUÍ

        public bool EnviarCorreo(string correoDestino, string asunto, string mensajeHtml)
        {
            try
            {
                var mail = new MailMessage();
                mail.From = new MailAddress(_correoOrigen, "Sistema Inclusivo UISEK");
                mail.To.Add(correoDestino);
                mail.Subject = asunto;
                mail.Body = mensajeHtml;
                mail.IsBodyHtml = true;

                var smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential(_correoOrigen, _claveOrigen);
                smtp.EnableSsl = true;

                smtp.Send(mail);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}