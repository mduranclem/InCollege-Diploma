using InCollege.Dominio.Modelos;
using InCollege.Dominio.Servicios;

namespace InCollege.Dominio.Patrones.Observer
{
    public class NotificadorEmail : IObserver
    {
        public void Notificar(Contrato contrato)
        {
            string asunto = "";
            string mensaje = "";
            bool enviar = false;

            // 1. CASO FIRMADO (NUEVA VENTA)
            if (contrato.Estado == "Firmado")
            {
                asunto = $"✅ NUEVA VENTA: Contrato #{contrato.Codigo}";
                mensaje = $"<h1>¡Felicitaciones!</h1><p>El contrato del colegio <strong>{contrato.NombreColegio}</strong> se ha firmado exitosamente.</p><p>Total Venta: ${(contrato.CantidadEgresados * contrato.PrecioUnitario).ToString("N0")}</p>";
                enviar = true;
            }
            // 2. CASO PRODUCCIÓN (ENTRA A TALLER)
            else if (contrato.Estado == "En Producción")
            {
                asunto = $"⚙️ INICIO PRODUCCIÓN: Contrato #{contrato.Codigo}";
                mensaje = $"<h1>Aviso a Planta</h1><p>El colegio <strong>{contrato.NombreColegio}</strong> ha ingresado a etapa de producción.</p>";
                enviar = true;
            }
            // 3. CASO TERMINADO
            else if (contrato.Estado == "Entregado")
            {
                asunto = $"📦 ENTREGA FINALIZADA: Contrato #{contrato.Codigo}";
                mensaje = $"<h1>Ciclo Completado</h1><p>Se ha entregado el pedido de <strong>{contrato.NombreColegio}</strong>.</p>";
                enviar = true;
            }

            // Enviamos el mail real
            if (enviar)
            {
                // CORRECCIÓN AQUÍ: Agregamos "" como primer parámetro (destinatario vacío)
                // El servicio usará los correos fijos de los admins que configuraste.
                EmailService.Enviar("", asunto, mensaje);
            }
        }
    }
}