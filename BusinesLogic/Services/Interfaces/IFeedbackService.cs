
using Model.Models;
using Telegram.Bot.Types;

namespace BusinesLogic.Services.Interfaces
{
    public interface IFeedbackService
    {
        string Get(int professorId, int page);
        int GetRaiting(int professorId);
        int GetCountOfPages();
        bool Create(Message message,int professorId,int professorRate);
    }
}
