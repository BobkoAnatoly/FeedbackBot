
using Model.Models;

namespace BusinesLogic.Services.Interfaces
{
    public interface IFeedbackService
    {
        string Get(int professorId, int page);
        int GetRaiting(int professorId);
        int GetCountOfPages();
        bool Create();
    }
}
