using InCollege.Dominio.Modelos;
using InCollege.Dominio.Servicios; // Para usar AuditoriaFactory
using System.Collections.Generic;

namespace InCollege.Dominio.Patrones.Observer
{
    // 1. OBSERVADOR DE GERENCIA (Manda mails y audita ventas)
    public class NotificadorGerencia : IObserver
    {
        // Usamos una lista temporal para guardar los logs que generemos
        private readonly List<Auditoria> _logs;

        public NotificadorGerencia(List<Auditoria> logs)
        {
            _logs = logs;
        }

        public void Notificar(Contrato contrato)
        {
            if (contrato.Estado == "Firmado")
            {
                // A. Simulación de envío de Email
                // (Aquí iría el código de EmailService.Enviar(...) si lo tuviéramos activado)

                // B. Generamos el registro de Auditoría
                var log = AuditoriaFactory.Crear(
                    "SISTEMA_OBSERVER",
                    "AVISO_GERENCIA",
                    $"Se notificó nueva venta: Contrato #{contrato.Codigo} por ${contrato.PrecioUnitario * contrato.CantidadEgresados}"
                );
                _logs.Add(log);
            }
            else if (contrato.Estado == "Perdido")
            {
                var log = AuditoriaFactory.Crear(
                    "SISTEMA_OBSERVER",
                    "ALERTA_PERDIDA",
                    $"URGENTE: Se cayó el contrato #{contrato.Codigo}. Revisar motivos."
                );
                _logs.Add(log);
            }
        }
    }

    // 2. OBSERVADOR DE TALLER (Solo le interesa si hay que trabajar)
    public class AlertaProduccion : IObserver
    {
        private readonly List<Auditoria> _logs;

        public AlertaProduccion(List<Auditoria> logs)
        {
            _logs = logs;
        }

        public void Notificar(Contrato contrato)
        {
            // El taller solo se mueve si el contrato se firmó
            if (contrato.Estado == "Firmado")
            {
                var log = AuditoriaFactory.Crear(
                    "SISTEMA_OBSERVER",
                    "ORDEN_TALLER",
                    $"Se generó orden de producción para {contrato.CantidadEgresados} unidades del {contrato.NombreProducto}."
                );
                _logs.Add(log);
            }
        }
    }
}