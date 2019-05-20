﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveyApp.API.Models
{
    public class SubmissionRequest
    {
        public int UserId { get; set; }
        public int SurveyId { get; set; }
        public List<int> QuestionChoicesIds { get; set; }
    }
}
