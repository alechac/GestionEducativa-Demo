using GestionEducativa.Views.Interfaces;

namespace GestionEducativa.Presenters
{
    public class MainPresenter
    {
        private readonly IMainView _view;

        public MainPresenter(IMainView view)
        {
            _view = view;
        }
    }
}
