using BusinesLogic.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Model;
using Model.Models;
using Telegram.Bot.Types;

namespace BusinesLogic.Services.Implementations
{
    public class FeedbackService : IFeedbackService
    {
        private const int FEEDBACKS_PER_PAGE = 1;
        private readonly ApplicationDatabaseContext _context;
        public FeedbackService(ApplicationDatabaseContext context)
        {
            _context = context;
        }
        public string Get(int professorId, int page)
        {
            double countOfPages = GetCountOfPages();
            double countOfFeedbacks = GetFeedbacksCount();
            var feedbacks = _context.Feedbacks.AsNoTracking().Where(x => x.ProfessorId == professorId && x.CreationDate > DateTime.Now.AddYears(-1)).ToList();
            var text = "";
            if (page < countOfPages)
            {
                for (int i = FEEDBACKS_PER_PAGE * page - FEEDBACKS_PER_PAGE; i < page * FEEDBACKS_PER_PAGE; i++)
                {
                    text += $"{i + 1}. {feedbacks[i].Text}\n" +
                        $"----------------------\n";
                }
                return text;
            }
            else
            {
                for (int i = FEEDBACKS_PER_PAGE * page - FEEDBACKS_PER_PAGE; i < countOfFeedbacks; i++)
                {
                    text += $"{i + 1}. {feedbacks[i].Text}\n" +
                        $"----------------------\n";
                }
                return text;
            }

        }

        public int GetRaiting(int professorId)
        {
            var raiting = _context.Feedbacks.AsNoTracking().Sum(x => x.Rating) / _context.Feedbacks.AsNoTracking().Count();
            return raiting;
        }
        public double GetFeedbacksCount()
        {
            var count = (double)_context.Feedbacks.AsNoTracking().Count();
            return count;
        }
        public int GetCountOfPages()
        {
            return (int)Math.Ceiling(GetFeedbacksCount()/FEEDBACKS_PER_PAGE);
        }
        public bool Create(Message message,int professorId,int professorRate)
        {
            var IsValid = ValidateFeedback(message.Text);
            if (IsValid)
            {
                var feedback = new Feedback
                {
                    Text = message.Text,
                    CreationDate = DateTime.Now,
                    ProfessorId = professorId,
                    Rating = professorRate
                };
                _context.Feedbacks.Add(feedback);
                _context.SaveChanges();
                return true;
            }
            return false;
        }
        public bool ValidateFeedback(string text)
        {
            string lowerText = text.ToLower();
            string path = "./Resources/Матюки.txt";
            string[] readText;
            using (var sr = new StreamReader(path))
            {
                readText = sr.ReadToEnd().Split(" ",StringSplitOptions.RemoveEmptyEntries);
            }
            foreach (var textItem in readText)
            {
                if (text.Contains(textItem))
                {
                    return false;
                }
            }
            if (lowerText.Contains("лекции")|
                lowerText.Contains("практические занятия")|
                lowerText.Contains("конспект")|
                lowerText.Contains("экзамен"))
            {
                return true;
            }
            else
            {
                return false;
            }
            return true;
        }

    }
}
