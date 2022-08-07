using AutoMapper;
using BusinesLogic.Services.Interfaces;
using Common.Models;
using Microsoft.EntityFrameworkCore;
using Model;
using Model.Models;

namespace BusinesLogic.Services.Implementations
{
    public class ProfessorsSrvice : IProfessorsService
    {
        private const int PROFESSORS_PER_PAGE = 20;
        private readonly ApplicationDatabaseContext _context;
        private readonly IMapper _mapper;
        public ProfessorsSrvice(ApplicationDatabaseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public string GetProfessors(int page)
        {
            double countOfPages = GetCountOfPages();
            double countOfProfessors = GetCountOfProfessors();
            List<Professor> professors = _context.Professors.AsNoTracking().ToList();
            if (page < countOfPages)
            {
                string text = "";
                for (int i = PROFESSORS_PER_PAGE * page - PROFESSORS_PER_PAGE; i < PROFESSORS_PER_PAGE * page; i++)
                {

                    text += i + 1 + ". " + professors[i].LastName + " " + professors[i].FirstName + " " + professors[i].Patronymic + "\n";
                }
                return text;
            }
            else
            {
                string text = "";
                for (int i = PROFESSORS_PER_PAGE * page - PROFESSORS_PER_PAGE; i < countOfProfessors; i++)
                {

                    text += i + 1 + ". " + professors[i].LastName + " " + professors[i].FirstName + " " + professors[i].Patronymic + "\n";
                }
                return text;
            }

        }
        public int GetCountOfProfessors()
        {
            int professors = _context.Professors.AsNoTracking().Count();
            return professors;
        }
        public double GetCountOfPages()
        {
            return Math.Round((double)GetCountOfProfessors() / PROFESSORS_PER_PAGE);
        }
        public Professor Get(string text)
        {
            var professor = _context.Professors
                .Include(x=>x.Feedbacks).ToList()
                .FirstOrDefault(x => $"{x.FirstName}{x.LastName}{x.Patronymic}".ToLower().Contains(text.ToLower()));
            if (professor == null) throw new Exception();
            return professor;
        }
    }
}
