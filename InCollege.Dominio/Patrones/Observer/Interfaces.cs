using InCollege.Dominio.Modelos;

namespace InCollege.Dominio.Patrones.Observer
{
    // INTERFAZ OBSERVADOR:
    // Todo el que quiera "escuchar" noticias debe tener este método.
    public interface IObserver
    {
        void Notificar(Contrato contrato);
    }

    // INTERFAZ SUJETO:
    // El que genera la noticia (El Contrato) debe saber hacer esto.
    public interface ISuject
    {
        void AgregarObservador(IObserver observador);
        void QuitarObservador(IObserver observador);
        void Avisar(Contrato contrato);
    }
}