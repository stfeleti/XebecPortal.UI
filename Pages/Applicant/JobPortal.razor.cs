﻿using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using X.PagedList;
using XebecPortal.UI.Pages.Applicant.Models;
using XebecPortal.UI.Pages.HR;
using XebecPortal.UI.Shared;

namespace XebecPortal.UI.Pages.Applicant
{
    public partial class JobPortal
    {
        private string searchJob;
        private bool IsApplyHidden;
        private bool nextButton, preButton = true;
        private bool jobPortalIsHidden = false;
        private bool applicationFormIsHidden = true;
        private bool pageLoad;
        private bool isFilterContainAnyVal;
        private int jobId;
        private IList<Job> jobList = new List<Job>();
        private IList<Job> jobListFilter = new List<Job>();
        private Job displayJobDetail = new Job();
        private IPagedList<Job> jobPagedList = new List<Job>().ToPagedList();
        private IList<Application> applicationList = new List<Application>();
        private List<JobType> JobTypes;
        private List<JobTypeHelper> jobTypeHelper;
        private List<Status> status;
        List<FormQuestion> QuestionList = new List<FormQuestion>();
        List<QuestionType> Types = new List<QuestionType>();
        List<ApplicantQuestion> ApplicantAnswers = new List<ApplicantQuestion>();
        List<ApplicantAnswer> AnswerList = new List<ApplicantAnswer>();

        private IEnumerable<string> mudSelectLocation;
        private IEnumerable<string> mudSelectCompany;
        private IEnumerable<string> mudSelectDepartment;
        private IEnumerable<string> mudSelectJobType;

        private IJSObjectReference _jsModule;

        protected override async Task OnInitializedAsync()
        {
            JobTypes = await httpClient.GetFromJsonAsync<List<JobType>>("https://xebecapi.azurewebsites.net/api/JobType");
            jobList = await httpClient.GetFromJsonAsync<List<Job>>("https://xebecapi.azurewebsites.net/api/Job");
            applicationList = await httpClient.GetFromJsonAsync<List<Application>>("https://xebecapi.azurewebsites.net/api/Application");
            status = await httpClient.GetFromJsonAsync<List<Status>>("/mockData/Status.json");
            jobTypeHelper = await httpClient.GetFromJsonAsync<List<JobTypeHelper>>("https://xebecapi.azurewebsites.net/api/JobTypeHelper");
            jobListFilter = jobList;
            jobPagedList = jobListFilter.ToPagedList(1, 17);
            displayJobDetail = jobListFilter.FirstOrDefault();
            DisplayJobDetail(displayJobDetail.Id);
            jobPortalIsHidden = false;
            applicationFormIsHidden = true;
            JobTypes = await httpClient.GetFromJsonAsync<List<JobType>>("https://xebecapi.azurewebsites.net/api/JobType");

            _jsModule = await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./jsPages/Applicant/JobPortal.js");
        }

        private async Task Apply(int id)
        {
            Application application = new()
            {
                TimeApplied = DateTime.Today,
                BeginApplication = DateTime.Today,
                JobId = id,
                AppUserId = 1
            };

            _ = await httpClient.PostAsJsonAsync("https://xebecapi.azurewebsites.net/api/ApplicationModel", application);
            jobList = await httpClient.GetFromJsonAsync<List<Job>>("https://xebecapi.azurewebsites.net/api/Job");
            applicationList = await httpClient.GetFromJsonAsync<List<Application>>("https://xebecapi.azurewebsites.net/api/Application");

            jobPortalIsHidden = true;
            applicationFormIsHidden = false;

            IsApplyHidden = true;
            Types = await httpClient.GetFromJsonAsync<List<QuestionType>>("https://xebecapi.azurewebsites.net/api/answertype");
            QuestionList = await httpClient.GetFromJsonAsync<List<FormQuestion>>($"https://xebecapi.azurewebsites.net/api/questionnaire/job/{id}");

            foreach (var q in QuestionList)
            {
                ApplicantQuestion tempAppQuestion = new ApplicantQuestion();
                tempAppQuestion.HRQuestion = q.question;
                tempAppQuestion.HRQuestionId = q.id;
                tempAppQuestion.TypeId = q.answerTypeId;

                ApplicantAnswers.Add(tempAppQuestion);
            }
        }

        private async Task PageListNav(int value)
        {
            jobPagedList = jobListFilter.ToPagedList(value, 17);
            nextButton = value == jobPagedList.PageCount || jobPagedList.PageCount == 1;
            preButton = value == 1;
            await _jsModule.InvokeVoidAsync("Scroll");
            DisplayJobDetail(jobPagedList.FirstOrDefault().Id);
        }

        private async Task SearchListJob(ChangeEventArgs e)
        {
            searchJob = e.Value.ToString();
            jobListFilter = jobList;
            FilterDataHelper();
            FilterDataDisplayHelper();
            await _jsModule.InvokeVoidAsync("Scroll");
        }

        private async Task SearchListLocation(IEnumerable<string> value)
        {
            mudSelectLocation = value;
            jobListFilter = jobList;
            FilterDataHelper();
            FilterDataDisplayHelper();
            await _jsModule.InvokeVoidAsync("Scroll");
        }
        private async Task SearchListCompany(IEnumerable<string> value)
        {
            mudSelectCompany = value;
            jobListFilter = jobList;
            FilterDataHelper();
            FilterDataDisplayHelper();
            await _jsModule.InvokeVoidAsync("Scroll");
        }

        private async Task SearchListDepartment(IEnumerable<string> value)
        {
            mudSelectDepartment = value;
            jobListFilter = jobList;
            FilterDataHelper();
            FilterDataDisplayHelper();
            await _jsModule.InvokeVoidAsync("Scroll");
        }

        private async Task SearchListJobType(IEnumerable<string> value)
        {
            mudSelectJobType = value;
            jobListFilter = jobList;
            FilterDataHelper();
            FilterDataDisplayHelper();
            await _jsModule.InvokeVoidAsync("Scroll");
        }

        private void DisplayJobDetail(int id)
        {
            IsApplyHidden = applicationList.Any(x => x.AppUserId == 1 && x.JobId == id);
            displayJobDetail = jobListFilter.FirstOrDefault(x => x.Id == id);
        }

        private void GoToApplicationForm()
        {
            applicationFormIsHidden = false;
            jobPortalIsHidden = true;
        }

        private void ToJobPortal()
        {
            applicationFormIsHidden = true;
            jobPortalIsHidden = false;
        }

        private string GetStyling(Job item)
        {
            if (displayJobDetail.Id == item.Id)
                return "box-shadow: rgba(0,51,64,0.86) 0px 0px 0px 2px, rgba(6, 24, 44, 0.65) 0px 4px 6px -1px, rgba(255, 255, 255, 0.08) 0px 1px 0px inset;";
            return "";
        }

        private string GetJobType(int id)
        {
            int jobTypeId = jobTypeHelper.FirstOrDefault(x => x.JobId == id).JobTypeId;
            return JobTypes.FirstOrDefault(x => x.Id == jobTypeId).Type;
        }

        private static string GetMultiSelectionTextLocation(List<string> selectedValues)
        {
            return $"Selected Location{(selectedValues.Count > 1 ? "s" : " ")}: {string.Join(", ", selectedValues.Select(x => x))}";
        }

        private static string GetMultiSelectionTextCompany(List<string> selectedValues)
        {
            return $"Selected Compan{(selectedValues.Count > 1 ? "ies" : "y")}: {string.Join(", ", selectedValues.Select(x => x))}";
        }

        private static string GetMultiSelectionTextDepartment(List<string> selectedValues)
        {
            return $"Selected Department{(selectedValues.Count > 1 ? "s" : " ")}: {string.Join(", ", selectedValues.Select(x => x))}";
        }

        private static string GetMultiSelectionTextJobType(List<string> selectedValues)
        {
            return $"Selected Job Type{(selectedValues.Count > 1 ? "s" : " ")}: {string.Join(", ", selectedValues.Select(x => x))}";
        }

        private void FilterDataHelper()
        {
            if (!string.IsNullOrEmpty(searchJob) && searchJob != " ")
            {
                jobListFilter = jobListFilter.Where(x => x.Title.Contains(searchJob, StringComparison.CurrentCultureIgnoreCase)).ToList();
            }

            if (mudSelectLocation?.Any() == true)
            {
                var listLocations = jobListFilter.Select(x => x.Location).Except(mudSelectLocation).ToList();
                jobListFilter = jobListFilter.Where(x => !listLocations.Contains(x.Location)).ToList();
            }

            if (mudSelectCompany?.Any() == true)
            {
                var listCompany = jobListFilter.Select(x => x.Company).Except(mudSelectCompany).ToList();
                jobListFilter = jobListFilter.Where(x => !listCompany.Contains(x.Company)).ToList();
            }

            if (mudSelectDepartment?.Any() == true)
            {
                var listDepartments = jobListFilter.Select(x => x.Department).Except(mudSelectDepartment).ToList();
                jobListFilter = jobListFilter.Where(x => !listDepartments.Contains(x.Department)).ToList();
            }

            if (mudSelectJobType?.Any() == true)
            {
                var listjobTypeIds = JobTypes.Where(x => mudSelectJobType.Contains(x.Type)).Select(x => x.Id);
                var listJobIds = jobTypeHelper.Where(x => listjobTypeIds.Contains(x.JobTypeId)).Select(x => x.JobId);
                jobListFilter = jobListFilter.Where(x => listJobIds.Contains(x.Id)).ToList();
            }
        }

        private void FilterDataDisplayHelper()
        {
            jobPagedList = jobListFilter.ToPagedList(1, 17);
            nextButton = jobPagedList.PageNumber == jobPagedList.PageCount;
            preButton = jobPagedList.PageNumber == 1;
            displayJobDetail = jobListFilter.FirstOrDefault();
        }

        public async Task SaveAnswers()
        {
            double tempScore;
            double tempMatches = 0;

            foreach (var q in ApplicantAnswers)
            {
                ApplicantAnswer tempAnswer = new ApplicantAnswer();
                tempAnswer.applicantAnswer = q.Applicantanswer;
                tempAnswer.questionnaireHRFormId = q.HRQuestionId;
                tempAnswer.appUserId = state.AppUserId;

                AnswerList.Add(tempAnswer);
            }

            await httpClient.PostAsJsonAsync<List<ApplicantAnswer>>("https://xebecapi.azurewebsites.net/api/applicantquestionnaire/list", AnswerList);

            for (int i = 0; i < QuestionList.Count; i++)
            {

                if (QuestionList[i].question.Contains("How many years of experience do you have"))
                {
                    if (int.Parse(QuestionList[i].answer) <= int.Parse(AnswerList[i].applicantAnswer))
                    {
                        tempMatches++;
                    }
                }
                else if (QuestionList[i].question == "What salary are you expecting?")
                {
                    if (int.Parse(QuestionList[i].answer) >= int.Parse(AnswerList[i].applicantAnswer))
                    {
                        tempMatches++;
                    }
                }
                else if (QuestionList[i].answer == AnswerList[i].applicantAnswer)
                {
                    tempMatches++;
                }
            }

            tempScore = tempMatches / QuestionList.Count * 100;

            CandidateRecommender candidateScore = new CandidateRecommender();
            candidateScore.jobId = jobId;
            candidateScore.AppUserId = state.AppUserId;
            candidateScore.TotalMatch = tempScore;

            //await httpClient.PostAsJsonAsync("https://xebecapi.azurewebsites.net/api/applicantquestionnaire", candidateScore);
        }
    }
}