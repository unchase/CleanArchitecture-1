﻿using Microsoft.JSInterop;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorHero.CleanArchitecture.AuditService.Interfaces.Responses;

namespace BlazorHero.CleanArchitecture.Client.Pages.Utilities
{
    public partial class AuditTrails
    {
        public List<RelatedAuditTrail> Trails = new List<RelatedAuditTrail>();
        private RelatedAuditTrail Trail = new RelatedAuditTrail();
        private string searchString = "";
        private bool _dense = true;
        private bool _striped = true;
        private bool _bordered = false;
        private bool _searchInOldValues = false;
        private bool _searchInNewValues = false;
        private MudDateRangePicker _dateRangePicker;
        private DateRange _dateRange;

        private bool Search(AuditResponse response)
        {
            var result = false;

            // check Search String
            if (string.IsNullOrWhiteSpace(searchString)) result = true;
            if (!result)
            {
                if (response.TableName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true)
                {
                    result = true;
                }
                if (_searchInOldValues &&
                    response.OldValues?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true)
                {
                    result = true;
                }
                if (_searchInNewValues &&
                    response.NewValues?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true)
                {
                    result = true;
                }
            }

            // check Date Range
            if (_dateRange?.Start == null && _dateRange?.End == null) return result;
            if (_dateRange?.Start != null && response.DateTime < _dateRange.Start)
            {
                result = false;
            }
            if (_dateRange?.End != null && response.DateTime > _dateRange.End + new TimeSpan(0,11, 59, 59, 999))
            {
                result = false;
            }

            return result;
        }

        protected override async Task OnInitializedAsync()
        {
            await GetDataAsync();
        }

        private async Task GetDataAsync()
        {
            var response = await _auditManager.GetCurrentUserTrailsAsync();
            if (response.Succeeded)
            {
                Trails = response.Data
                    .Select(x => new RelatedAuditTrail
                    {
                        AffectedColumns = x.AffectedColumns,
                        DateTime = x.DateTime,
                        Id = x.Id,
                        NewValues = x.NewValues,
                        OldValues = x.OldValues,
                        PrimaryKey = x.PrimaryKey,
                        TableName = x.TableName,
                        Type = x.Type,
                        UserId = x.UserId,
                        LocalTime = DateTime.SpecifyKind(x.DateTime, DateTimeKind.Utc).ToLocalTime()
                    }).ToList();
            }
            else
            {
                foreach (var message in response.Messages)
                {
                    _snackBar.Add(localizer[message], Severity.Error);
                }
            }
        }

        private void ShowBtnPress(int id)
        {
            Trail = Trails.First(f => f.Id == id);
            foreach (var trial in Trails.Where(a => a.Id != id))
            {
                trial.ShowDetails = false;
            }
            Trail.ShowDetails = !Trail.ShowDetails;
        }

        private async Task ExportToExcelAsync()
        {
            var base64 = await _auditManager.DownloadFileAsync();
            await _jsRuntime.InvokeVoidAsync("Download", new
            {
                ByteArray = base64,
                FileName = $"audit_trails_{DateTime.Now:ddMMyyyyHHmmss}.xlsx",
                MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            });
        }

        public class RelatedAuditTrail : AuditResponse
        {
            public bool ShowDetails { get; set; } = false;
            public DateTime LocalTime { get; set; }
        }
    }
}