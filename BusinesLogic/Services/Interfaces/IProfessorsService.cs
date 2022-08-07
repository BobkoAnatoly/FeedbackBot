
using Common.Models;
using Model.Models;

namespace BusinesLogic.Services.Interfaces
{
    public interface IProfessorsService
    {
        public string GetProfessors(int page);
        int GetCountOfProfessors();
        Professor Get(string text);
    }
}
