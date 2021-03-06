﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SurveyApp.API.Data;
using SurveyApp.API.Data.Entities;
using SurveyApp.API.Models;
using SurveyApp.API.Services;

namespace SurveyApp.API.Controllers
{
    [Route("api/surveys")]
    [ApiController]
    public class SurveysController : BaseController
    {
        public SurveysController(AppDbContext context, IMapper mapper, NotificationService notificationService) : base(context, mapper, notificationService)
        {
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var surveys = await Context.Surveys.Include(s => s.Questions).ThenInclude(q => q.Question).ThenInclude(q => q.Choices).Include(s => s.Submissions).ToListAsync();

            var result = Mapper.Map<List<SurveyResponse>>(surveys);

            NotificationService.Send($"User {NotificationService.UserName} fetched all surveys.");

            return Ok(result);
        }


        [HttpGet]
        [Authorize]
        [Route("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var survey = await Context.Surveys.Include(s => s.Questions).ThenInclude(q => q.Question).ThenInclude(q => q.Choices).Include(s => s.Submissions).FirstOrDefaultAsync(s => s.Id == id);
            var result = Mapper.Map<SurveyResponse>(survey);
            NotificationService.Send($"User {NotificationService.UserName} fetched survey with id {id}.");
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]SurveyRequest request)
        {
            var survey = Mapper.Map<SurveyDb>(request);
            var questions = await Context.Questions.Where(q => request.QuestionIds.Contains(q.Id)).ToListAsync();
            Context.Surveys.Add(survey);
            await Context.SaveChangesAsync();
            survey = await Context.Surveys.Include(s => s.Questions).FirstOrDefaultAsync(s => s.Id == survey.Id);
            questions.ForEach(q => survey.Questions.Add(new SurveyQuestionDb { Survey = survey, Question = q }));
            await Context.SaveChangesAsync();
            survey = await Context.Surveys.Include(s => s.Questions).ThenInclude(q => q.Question).ThenInclude(q => q.Choices).Include(s => s.Submissions)
                .FirstOrDefaultAsync(s => s.Id == survey.Id);

            var result = Mapper.Map<SurveyResponse>(survey);
            NotificationService.Send($"User {NotificationService.UserName} posted a new survey.");

            return Ok(result);
        }
    }
}