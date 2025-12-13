using InCollege.Dominio.Modelos;
using System.Collections.Generic;

namespace InCollege.Dominio.Patrones.Observer
{
    public class GestorNotificaciones : ISuject
    {
        private List<IObserver> _suscriptores = new List<IObserver>();

        public void AgregarObservador(IObserver observador)
        {
            _suscriptores.Add(observador);
        }

        public void QuitarObservador(IObserver observador)
        {
            _suscriptores.Remove(observador);
        }

        // Este es el método que dispara todo en cadena
        public void Avisar(Contrato contrato)
        {
            foreach (var obs in _suscriptores)
            {
                obs.Notificar(contrato);
            }
        }
    }
}