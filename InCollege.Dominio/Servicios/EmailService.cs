using System.Net;
using System.Net.Mail;
using System;

namespace InCollege.Dominio.Servicios
{
    public static class EmailService
    {
        // --- CORRIGE ESTOS DATOS ---

        // 1. Aquí debe ir TU GMAIL REAL (con el que creaste la clave)
        // NO pongas "administracion@incollege.com" si esa cuenta no existe en Google.
        private static string Remitente = "mduranclem@gmail.com";

        // 2. Tu contraseña de aplicación de 16 letras
        private static string Password = "pqomqngblmkgqbvo";

        private static string SmtpHost = "smtp.gmail.com";
        private static int SmtpPort = 587;

        public static void Enviar(string destinatario, string asunto, string cuerpo)
        {
            try
            {
                using (SmtpClient client = new SmtpClient(SmtpHost, SmtpPort))
                {
                    client.EnableSsl = true;

                    // CORRECCIÓN DE ORDEN (CRÍTICO PARA QUE FUNCIONE)
                    // Primero decimos "No uses credenciales por defecto"
                    client.UseDefaultCredentials = false;

                    // Segundo, le damos las credenciales reales
                    client.Credentials = new NetworkCredential(Remitente, Password);

                    MailMessage mail = new MailMessage();

                    // Aquí es donde hacemos el truco para que se vea bonito
                    // Aunque el remitente real es tu gmail, el "Nombre a mostrar" será el de la empresa
                    mail.From = new MailAddress(Remitente, "Administración InCollege");

                    // Destinatarios
                    if (!string.IsNullOrEmpty(destinatario))
                    {
                        mail.To.Add(destinatario);
                    }

                    // Tus correos de admin
                    mail.To.Add("mduranclem@gmail.com");
                    mail.To.Add("matuuduranlalo@gmail.com");

                    mail.Subject = asunto;
                    mail.Body = cuerpo;
                    mail.IsBodyHtml = true;

                    client.Send(mail);
                }
            }
            catch (Exception ex)
            {
                string errorDetalle = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                throw new Exception($"ERROR DE MAIL: {errorDetalle}");
            }
        }
    }
}